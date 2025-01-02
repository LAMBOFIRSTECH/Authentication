using Microsoft.AspNetCore.Mvc;
using Authentifications.Services;
using Authentifications.Interfaces;
namespace Authentifications.Controllers;
[Route("api/v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationService jwtToken;
	//private readonly IRedisCacheService redisCache;
	private readonly ILogger<JwtBearerAuthenticationService> log;
	public AccessTokenController(ILogger<JwtBearerAuthenticationService> log, IRedisCacheService redisCache, JwtBearerAuthenticationService jwtToken)
	{
		this.jwtToken = jwtToken;
		this.log = log;
		//this.redisCache = redisCache;
	}
	[HttpPost("login")]
	public async Task<ActionResult> Authentificate()
	{
		if (!User.Identity!.IsAuthenticated) 
		{
			return Unauthorized("Unauthorized access");
		}
		var email = HttpContext.Items["email"] as string;
		var password = HttpContext.Items["password"] as string;
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
			return BadRequest("Email or password is missing.");	
		var user = await jwtToken.AuthUserDetailsAsync((User.Identity!.IsAuthenticated, email,password));
		var result = await jwtToken.GetToken(user);
		if (!result.Response)
		{
			return Unauthorized(new { result.Message });
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