using Microsoft.AspNetCore.Mvc;
using Authentifications.Services;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Authentifications.Interfaces;
namespace Authentifications.Controllers;
[Route("api/v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationService jwtToken;
	private readonly IRedisCacheService redisCache;
	private readonly HttpClient _httpClient;
	private readonly ILogger<JwtBearerAuthenticationService> log;
	public AccessTokenController(ILogger<JwtBearerAuthenticationService> log, IRedisCacheService redisCache, JwtBearerAuthenticationService jwtToken, HttpClient httpClient)
	{
		this.jwtToken = jwtToken;
		_httpClient = httpClient;
		this.log = log;
		this.redisCache = redisCache;
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

		// Step 2: Encode the credentials in Base64
		string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
		string authorizationHeader = $"Basic {base64Credentials}";
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7103/api/v1/login")
		{
			Headers = { { "Authorization", authorizationHeader } }
		};
		var response = await _httpClient.SendAsync(requestMessage);
		await response.Content.ReadAsStringAsync();
		
		await redisCache.GetDataFromRedisByFilterAsync(email,password); 
		
		log.LogInformation("Authentication successful");
		//Avant meme de générer un token se ressurer qu'il est présent dans redis et qu'il n'a pas été révoqué avant (d'ou la blacklist des sessions de token revoqué dans redis)
		//On peut aussi ajouter un champ dans la base de données pour savoir si le token est révoqué ou pas
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
		var result=await redisCache.GetDataFromRedisByFilterAsync(email,password);
		if(result is false)
		{
			log.LogError($"Not found email {email}");
			return NotFound($"Not found email {email}");
		}
		return Ok("user found");
	}
}