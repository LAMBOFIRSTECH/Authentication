using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

using Authentifications.Repositories;
using Authentifications.Services;
using System.Text;
namespace Authentifications.Controllers;
[Route("api/v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationService jwtToken;
	// private readonly ApiContext context; // Que pour les tests ne pas faire ceci dans un controller
	private readonly AuthentificationBasicService basic;
	private readonly RedisCacheService redis;
	private readonly HttpClient _httpClient;
	private readonly ILogger<RedisCacheService> log;
	public AccessTokenController(ILogger<RedisCacheService> log, JwtBearerAuthenticationService jwtToken, AuthentificationBasicService basic, RedisCacheService redis, HttpClient httpClient)
	{
		this.jwtToken = jwtToken;
		this.basic = basic;
		this.redis = redis;
		_httpClient = httpClient;
		this.log = log;
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
		string credentials = $"{email}:{password}";

		// Step 2: Encode the credentials in Base64
		string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
		string authorizationHeader = $"Basic {base64Credentials}";
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7103/api/v1/login")
		{
			Headers = { { "Authorization", authorizationHeader } }
		};
		var response = await _httpClient.SendAsync(requestMessage);
		if (response.IsSuccessStatusCode == false)
		{
			return Unauthorized(new { Message = "Invalid credentials" });
		}
		await response.Content.ReadAsStringAsync();
		log.LogInformation("Authentication successful");
		var result = await jwtToken.GetToken(email, password);
		if (!result.Response)
		{
			return Unauthorized(new { result.Message });
		}
		return CreatedAtAction(nameof(Authentificate), new { result.Token });
	}
	//[Authorize]
	// [HttpGet("users")]
	// public async Task<ActionResult> Users()
	// {
	// 	return Ok(await context.GetUsersDataAsync());
	// }
}