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
using System.Security.Claims;
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
		string email = User.Claims.Where(c => c.Type == ClaimTypes.Email).Select(c => c.Value).FirstOrDefault()!;
		if (!ModelState.IsValid)
			return BadRequest(ModelState);
		if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(utilisateurDto.Pass))
			return BadRequest("Email or password is missing.");
		if (!utilisateurDto.CheckEmailAdress(email))
			return BadRequest($"Invalid email");


		var user = await jwtToken.AuthUserDetailsAsync((ModelState.IsValid, utilisateurDto));
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