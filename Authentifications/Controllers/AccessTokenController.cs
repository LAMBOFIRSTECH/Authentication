using Microsoft.AspNetCore.Mvc;
using Authentifications.Middlewares;
using Authentifications.Interfaces;
using Microsoft.AspNetCore.Authentication;
using System;
namespace Authentifications.Controllers;
[Route("v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationMiddleware jwtToken;
	private readonly IRedisCacheService redisCache;
	private readonly ILogger<JwtBearerAuthenticationMiddleware> log;
	public AccessTokenController(ILogger<JwtBearerAuthenticationMiddleware> log, IRedisCacheService redisCache, JwtBearerAuthenticationMiddleware jwtToken)
	{
		this.jwtToken = jwtToken;
		this.log = log;
		this.redisCache = redisCache;
	}
	[HttpPost("login")]
	public async Task<ActionResult> Authentificate()
	{
		var scheme = await HttpContext.AuthenticateAsync("Basic");
		var options = new RemoteAuthenticationOptions();
		var ticket = new AuthenticationTicket(User, "Basic");
		TicketReceivedContext ticketReceivedContext = new(HttpContext, new AuthenticationScheme("Basic", "Basic", typeof(AuthentificationBasicMiddleware)), options, ticket);
		ticketReceivedContext.Success();
		ticket.Properties.Items["email"] = HttpContext.Items["email"] as string;
		ticket.Properties.Items["password"] = HttpContext.Items["password"] as string;
		
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