using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using Authentifications.DataBaseContext;
using Authentifications.Repositories;
using Authentifications.Services;
namespace Authentifications.Controllers;
[Route("api/v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationService jwtToken;
	private readonly ApiContext context; // Que pour les tests ne pas faire ceci dans un controller
	private readonly AuthentificationBasicService basic;
	private readonly RedisCacheService redis;
	public AccessTokenController(JwtBearerAuthenticationService jwtToken, ApiContext context, AuthentificationBasicService basic, RedisCacheService redis)
	{
		this.jwtToken = jwtToken;
		this.context = context;
		this.basic = basic;
		this.redis = redis;
	}
	/// <param name="email"></param>
	/// <param name="password"></param> 
	/// <returns></returns>
	[HttpPost("login")]
	public async Task<ActionResult> Authentificate([EmailAddress] string email, [DataType(DataType.Password)] string password)
	{
		if (!ModelState.IsValid)
		{
			// Ajouter les erreurs de validation dans HttpContext.Items
			var validationErrors = ModelState.Values
				.SelectMany(v => v.Errors)
				.Select(e => e.ErrorMessage)
				.ToList();
			HttpContext.Items["ModelValidationErrors"] = validationErrors;
			return StatusCode(422);
		}
		var resultat= await redis.StoreCredentialsAsync(email, password);
		if(!resultat)
		{
			return StatusCode(503, "Le service est temporairement indisponible. Réessayez plus tard.");
		}
		var isAuthenticated = await basic.AuthenticateAsync(email, password);
		if (!isAuthenticated)
		{
			return Unauthorized(new { Errors = "Invalid email or password" });
		}
		var result = await jwtToken.GetToken(email,password);
		if (!result.Response)
		{
			return Unauthorized(new { result.Message });
		}
		return CreatedAtAction(nameof(Authentificate), new { result.Token });
	}
	//[Authorize]
	[HttpGet("users")]
	public async Task<ActionResult> Users()
	{
		return Ok(await context.GetUsersDataAsync());
	}
}