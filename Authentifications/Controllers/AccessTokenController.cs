using Microsoft.AspNetCore.Mvc;
using Authentifications.Services;
using Authentifications.Interfaces;
using Authentifications.Models;
using System.Text;
using Microsoft.OpenApi.Expressions;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using static Authentifications.Models.UtilisateurDto;
using System.Net.Http.Headers;
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
		if (!User.Identity!.IsAuthenticated)  // Il faut générer un ticket pour le user authentifié dans basic auth
		{
			return Unauthorized("Unauthorized access");
		}
		string email = User.Identity.Name!;
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(utilisateurDto.Pass))
			return BadRequest("Email or password is missing.");
		if (!utilisateurDto.CheckEmailAdress(email))
			return BadRequest($"Invalid email");

		// string credentials = $"{loginRequest.Email}:{loginRequest.Pass}";
		// // Step 2: Encode the credentials in Base64
		// string base64Credentials = Convert.ToBase64String(Encoding.UTF8.GetBytes(credentials));
		// string authorizationHeader = $"Basic {base64Credentials}";
		// using var client = new HttpClient();
		// client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Basic", base64Credentials);
		// var bodyContent= new 
		// {
		// 	loginRequest.Email,
		// 	loginRequest.Pass,
		// 	State = ModelState.IsValid
		// };
		// var jsonContent = new StringContent(JsonSerializer.Serialize(bodyContent), Encoding.UTF8, "application/json");

		// var response = await client.PostAsync("https://localhost:7103/api/v1/login", jsonContent);
		// if (response.IsSuccessStatusCode == false)
		// {
		// 	return Unauthorized(new { Message = "Invalid credentials" });
		// }
		// await response.Content.ReadAsStringAsync();
		var user = await jwtToken.BasicAuthResponseAsync((ModelState.IsValid, utilisateurDto));
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