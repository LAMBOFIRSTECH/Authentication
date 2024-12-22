using Microsoft.AspNetCore.Mvc;
using Authentifications.Services;
using System.Text;
using Authentifications.Models;
namespace Authentifications.Controllers;
[Route("api/v1/")]
public class AccessTokenController : ControllerBase
{
	private readonly JwtBearerAuthenticationService jwtToken;
	private readonly HttpClient _httpClient;
	private readonly ILogger<JwtBearerAuthenticationService> log;
	public AccessTokenController(ILogger<JwtBearerAuthenticationService> log, JwtBearerAuthenticationService jwtToken, HttpClient httpClient)
	{
		this.jwtToken = jwtToken;
		_httpClient = httpClient;
		this.log = log;
	}
	[HttpPost("login")]
	public async Task<ActionResult> Authentificate([FromBody] LoginRequest login)
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
		string credentials = $"{login.Email}:{login.Pass}";

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
		var result = await jwtToken.GetToken(login.Email!);
		if (!result.Response)
		{
			return Unauthorized(new { result.Message });
		}
		return CreatedAtAction(nameof(Authentificate), new { result.Token });
	}
}