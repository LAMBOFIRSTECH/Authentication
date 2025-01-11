using Microsoft.AspNetCore.Mvc;
using Authentifications.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Authentifications.Services;
using Authentifications.Models;
using Microsoft.AspNetCore.Authorization;
namespace Authentifications.Controllers;
[Route("api/auth")]
public class TokenController : ControllerBase
{
	private readonly JwtAccessAndRefreshTokenService jwtToken;
	private readonly IRedisCacheTokenService redisTokenCache;
	private readonly IRedisCacheService redisCache;
	private readonly ILogger<JwtAccessAndRefreshTokenService> log;
	private string? adress_mail;
	private string? pass;
	public TokenController(ILogger<JwtAccessAndRefreshTokenService> log, IRedisCacheTokenService redisTokenCache, JwtAccessAndRefreshTokenService jwtToken, IRedisCacheService redisCache)
	{
		this.jwtToken = jwtToken;
		this.log = log;
		this.redisTokenCache = redisTokenCache;
		this.redisCache = redisCache;
	}
	/// <summary>
	/// Authentifie un utilisateur et retourne les tokens (access et refresh).
	/// </summary>
	[HttpPost("login")]
	public async Task<ActionResult> Authentificate()
	{
		var email = HttpContext.Items["email"] as string;
		var password = HttpContext.Items["password"] as string;
		var ticket = new AuthenticationTicket(User, "Basic");
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			return BadRequest("Email or password is missing.");
		if (!User.Identity!.IsAuthenticated)
			return Unauthorized("Unauthorized access");

		var user = await jwtToken.AuthUserDetailsAsync((User.Identity!.IsAuthenticated, email, password));
		var result = jwtToken.GetToken(user);
		if (user.Pass == null)
			return BadRequest("Password is missing.");
		if (!result.Response)
			return Unauthorized(new { result.Message });

		adress_mail = email;
		pass = password;

		log.LogWarning("Stored email: {Email} and password: {Password} in HttpContext.Items", email, password);

		return CreatedAtAction(nameof(Authentificate), new { result.Token, result.RefreshToken });
	}
	/// <summary>
	/// Rafraîchit le token en utilisant un refresh token valide.
	///<param name="refreshToken"></param>
	/// </summary>
	[HttpPut("refreshToken")]
	[AllowAnonymous]
	public async Task<ActionResult> RegenerateAccessTokenUsingRefreshToken([FromBody] string refreshToken)
	{
		
		log.LogWarning("Stored email: {Email} and password: {Password} in HttpContext.Items", adress_mail, adress_mail);
		if (string.IsNullOrWhiteSpace(adress_mail) || string.IsNullOrWhiteSpace(pass))
			return BadRequest("Email or password is missing.Could not refresh Token");
		var result = await jwtToken.NewAccessTokenUsingRefreshTokenAsync(refreshToken, adress_mail!, pass!);
		if (!result.Response)
			return Unauthorized(new { result.Message });
		return CreatedAtAction(nameof(RegenerateAccessTokenUsingRefreshToken), new { result.Token, result.RefreshToken });
	}
}