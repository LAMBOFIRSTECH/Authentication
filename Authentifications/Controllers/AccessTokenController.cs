using Microsoft.AspNetCore.Mvc;
using Authentifications.Services;
using System.Text;
using Authentifications.Models;
using Authentifications.RedisContext;
using System.ComponentModel.DataAnnotations;
namespace Authentifications.Controllers;
[Route("api/v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationService jwtToken;
	private readonly RedisCacheService redis;
	private readonly HttpClient _httpClient;
	private readonly ILogger<JwtBearerAuthenticationService> log;
	public AccessTokenController(ILogger<JwtBearerAuthenticationService> log, RedisCacheService redis, JwtBearerAuthenticationService jwtToken, HttpClient httpClient)
	{
		this.jwtToken = jwtToken;
		_httpClient = httpClient;
		this.log = log;
		this.redis = redis;
	}
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
			return StatusCode(415);
		}
		
		string credentials = $"{email}:{password}";
		//await redis.GetCachedValueAsync(credentials);

		// Step 2: Encode the credentials in Base64
		string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
		string authorizationHeader = $"Basic {base64Credentials}";
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7103/api/v1/login")
		{
			Headers = { { "Authorization", authorizationHeader } }
		};
		var response = await _httpClient.SendAsync(requestMessage);
		await response.Content.ReadAsStringAsync();
		log.LogInformation("Authentication successful");
		var result = await jwtToken.GetToken(email!);
		if (!result.Response)
		{
			return Unauthorized(new { result.Message });
		}
		return CreatedAtAction(nameof(Authentificate), new { result.Token });
	}

	[HttpGet("users")]
	public async Task<ActionResult> Get()
	{
		string email = "lambo@example.com";
		string password = "lambo";
		await redis.GenerateAsyncDataByFilter(email,password); //credentials
		return Ok("user found");
	}
}