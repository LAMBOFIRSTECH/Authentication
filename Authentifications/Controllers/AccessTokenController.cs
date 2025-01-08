using Microsoft.AspNetCore.Mvc;
using Authentifications.Middlewares;
using Authentifications.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Authentifications.Services;
namespace Authentifications.Controllers;
[Route("v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationService jwtToken;
	private readonly IRedisCacheTokenService redisTokenCache;
	private readonly ILogger<JwtBearerAuthenticationService> log;
	public AccessTokenController(ILogger<JwtBearerAuthenticationService> log, IRedisCacheTokenService redisTokenCache, JwtBearerAuthenticationService jwtToken)
	{
		this.jwtToken = jwtToken;
		this.log = log;
		this.redisTokenCache = redisTokenCache;
	}
	[HttpPost("login")]
	public async Task<ActionResult> Authentificate()
	{
		var ticket = new AuthenticationTicket(User, "Basic");
		var email = ticket.Properties.Items["email"] = HttpContext.Items["email"] as string;
		var password = ticket.Properties.Items["password"] = HttpContext.Items["password"] as string;
		if (!User.Identity!.IsAuthenticated)
		{
			return Unauthorized("Unauthorized access");
		}
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
		return CreatedAtAction(nameof(Authentificate), new { result.Token });
	}

	// [HttpGet("users")]
	// public async Task<ActionResult> Get()
	// {
	// 	string email = "lambo@example.com";
	// 	string password = "lambo";
	// 	var result = await redisCache.GetDataFromRedisByFilterAsync(email, password);
	// 	if (result is false)
	// 	{
	// 		return NotFound($"Not found email {email}");
	// 	}
	// 	return Ok("user found");
	// }
}