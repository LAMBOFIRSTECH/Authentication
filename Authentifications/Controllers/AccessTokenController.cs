using Microsoft.AspNetCore.Mvc;
using Authentifications.Services;
using System.Text;
using System.ComponentModel.DataAnnotations;
using Authentifications.Interfaces;
using Authentifications.Models;
using System.Reflection.Metadata.Ecma335;
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

		var tupleResult = await redisCache.GetDataFromRedisUsingParamsAsync(ModelState.IsValid, utilisateurDto.Email, utilisateurDto.Pass);
		log.LogInformation("Authentication successful");
		var utilisateur = tupleResult.Item2;
		//Avant meme de générer un token se ressurer qu'il est présent dans redis et qu'il n'a pas été révoqué avant (d'ou la blacklist des sessions de token revoqué dans redis)
		//On peut aussi ajouter un champ dans la base de données pour savoir si le token est révoqué ou pas
		var result = await jwtToken.GetToken(utilisateur);
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