using Microsoft.AspNetCore.Mvc;
using Authentifications.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Authentifications.Services;
using Authentifications.Models;
namespace Authentifications.Controllers;
[Route("api/auth")] 
public class TokenController : ControllerBase
{
    private readonly JwtAccessAndRefreshTokenService jwtToken;
    private readonly IRedisCacheTokenService redisTokenCache;
    private readonly ILogger<JwtAccessAndRefreshTokenService> log;
    public TokenController(ILogger<JwtAccessAndRefreshTokenService> log, IRedisCacheTokenService redisTokenCache, JwtAccessAndRefreshTokenService jwtToken)
    {
        this.jwtToken = jwtToken;
        this.log = log;
        this.redisTokenCache = redisTokenCache;
    }
    /// <summary>
    /// Authentifie un utilisateur et retourne les tokens (access et refresh).
    /// </summary>
    [HttpPost("login")]
    public async Task<ActionResult> Authentificate()
    {
        var ticket = new AuthenticationTicket(User, "Basic");
        var email = ticket.Properties.Items["email"] = HttpContext.Items["email"] as string;
        var password = ticket.Properties.Items["password"] = HttpContext.Items["password"] as string;

        if (!User.Identity!.IsAuthenticated)
            return Unauthorized("Unauthorized access");
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email or password is missing.");

        var user = await jwtToken.AuthUserDetailsAsync((User.Identity!.IsAuthenticated, email, password));
        var result = await jwtToken.GetToken(user);
        if (!result.Response)
        {
            return Unauthorized(new { result.Message });
        }
        if (user.Pass == null)
        {
            return BadRequest("Password is missing.");
        }
        return CreatedAtAction(nameof(Authentificate), new { result.Token, result.RefreshToken });
    }

    /// <summary>
    /// Rafraîchit les tokens en utilisant un refresh token valide.
    ///<param name="refreshToken"></param>
    /// </summary>
    [HttpPut("refreshToken")]
    public async Task<ActionResult> RegenerateAccessTokenUsingRefreshToken([FromBody] string refreshToken)
    {
        var email = HttpContext.Items["email"] as string;
        var password = HttpContext.Items["password"] as string;
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            return BadRequest("Email or password is missing.");
        var check = await jwtToken.CheckExistedJwtRefreshToken(refreshToken, email!, password!);
        if (check.Equals(false))
            return NotFound("Ce refresh token n'existe pas");

        UtilisateurDto user = new();
        var result = await jwtToken.GetToken(user); // Revoir ce controlleur
        return CreatedAtAction(nameof(RegenerateAccessTokenUsingRefreshToken), new { result.RefreshToken });
    }
}