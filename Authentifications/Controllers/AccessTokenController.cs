using Microsoft.AspNetCore.Mvc;
using Authentifications.Services;
using Authentifications.Interfaces;
using Authentifications.Models;
using System.Text;
using Microsoft.OpenApi.Expressions;
using System.Text.Json;
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
	public async Task<ActionResult> Authentificate([FromBody] UtilisateurDto utilisateurDto)
	{
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		if (utilisateurDto.Email is null || utilisateurDto.Pass is null)
			return BadRequest("Email or password is null");
		if (!utilisateurDto.CheckEmailAdress(utilisateurDto.Email))
			return BadRequest($"Invalid email");

		string credentials = $"{utilisateurDto.Email}:{utilisateurDto.Pass}";
		// Step 2: Encode the credentials in Base64
		string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
		string authorizationHeader = $"Basic {base64Credentials}";
		var modelStateJson = new
		{
			valideModelState = ModelState.IsValid,
			email = utilisateurDto.Email,
			password = utilisateurDto.Pass
		};
		string jsonContent = JsonSerializer.Serialize(modelStateJson);
		var requestMessage = new HttpRequestMessage(HttpMethod.Post, "https://localhost:7103/api/v1/login")
		{
			Headers = { { "Authorization", authorizationHeader } },
			Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
		};
		var response = await _httpClient.SendAsync(requestMessage);
		if (response.IsSuccessStatusCode == false)
		{
			return Unauthorized(new { Message = "Invalid credentials" });
		}
		await response.Content.ReadAsStringAsync();
		var user = await jwtToken.BasicAuthResponseAsync((ModelState.IsValid,utilisateurDto));

		// var tupleResult = await redisCache.GetDataFromRedisUsingParamsAsync(ModelState.IsValid, utilisateurDto.Email, utilisateurDto.Pass);
		// if (tupleResult.Item1 is false)
		// {
		// 	log.LogError("Authentication failed");
		// 	return Unauthorized($"Authentication failed, email adress or password is incorrect");
		// }
		log.LogInformation("Authentication successful");
		var result = await jwtToken.GetToken(user);
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
		var result = await redisCache.GetDataFromRedisByFilterAsync(email, password);
		if (result is false)
		{
			log.LogError($"Not found email {email}");
			return NotFound($"Not found email {email}");
		}
		return Ok("user found");
	}
}