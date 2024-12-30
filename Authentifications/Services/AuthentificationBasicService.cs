using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Authentifications.Interfaces;
using Authentifications.Models;
using Authentifications.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using static Authentifications.Models.UtilisateurDto;

namespace Authentifications.Repositories;
public class AuthentificationBasicService : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly JwtBearerAuthenticationService jwtBearerAuthenticationService;
	private readonly ILogger<JwtBearerAuthenticationService> log;
	private readonly IRedisCacheTokenService redisToken;
	private readonly IRedisCacheService redisCache;

	public AuthentificationBasicService(IRedisCacheTokenService redisToken, IRedisCacheService redisCache, JwtBearerAuthenticationService jwtBearerAuthenticationService, IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	ISystemClock clock, ILogger<JwtBearerAuthenticationService> log)
	: base(options, logger, encoder, clock)
	{
		this.jwtBearerAuthenticationService = jwtBearerAuthenticationService;
		this.log = log;
		this.redisToken = redisToken;
		this.redisCache = redisCache;

	}

	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		log.LogInformation("En-têtes de la requête : {Headers}", string.Join(", ", Request.Headers.Select(h => $"{h.Key}: {h.Value}")));

		if (!Request.Headers.ContainsKey("Authorization"))
			return AuthenticateResult.Fail("Authorization header missing");
		try
		{
			var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
			if (string.IsNullOrEmpty(authHeader.Parameter) || !authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
				return AuthenticateResult.Fail("Invalid Authorization header format");
			if (Request.ContentType == null || !Request.ContentType.Contains("application/json"))
				return AuthenticateResult.Fail("Invalid Content-Type. Expected 'application/json'.");

			var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
			if (credentials.Length != 2)
				return AuthenticateResult.Fail("Invalid Authorization header format");

			var email = credentials[0];
			var password = credentials[1];
			log.LogWarning($"Authentication ---------- {email}");
			log.LogWarning($"Authentication password----------{password}");
			//Request.EnableBuffering();
			using (StreamReader reader = new StreamReader(Request.Body, Encoding.UTF8, leaveOpen: true))
			{
				string jsonBody = await reader.ReadToEndAsync();
				//Request.Body.Position = 0;
				if (string.IsNullOrEmpty(jsonBody))
					return AuthenticateResult.Fail("Empty request body");
				log.LogWarning($"json----------{jsonBody}");
				// Désérialiser le JSON pour le manipuler
				var requestData = JsonSerializer.Deserialize<LoginRequest>(jsonBody);

				log.LogWarning("Authentication ---------- {Email}", requestData?.Email);

				if (requestData == null || string.IsNullOrEmpty(requestData.Email) || string.IsNullOrEmpty(requestData.Pass))
				{
					return AuthenticateResult.Fail("Invalid JSON body.");
				}
				log.LogWarning("Authentication ----------", requestData.Pass);
				log.LogWarning("Authentication ----------", requestData.State);

				//Avant meme de générer un token se ressurer qu'il est présent dans redis et qu'il n'a pas été révoqué avant (d'ou la blacklist des sessions de token revoqué dans redis)
				//On peut aussi ajouter un champ dans la base de données pour savoir si le token est révoqué ou pas
				// redisToken.GenerateRedisKeyForTokenSession(email, password);
				// redisToken.StoreTokenSessionInRedis(email);
				var tupleResult = await redisCache.GetDataFromRedisUsingParamsAsync(requestData.State, email, password);
				if (tupleResult.Item1 is false)
				{
					log.LogError("Authentication failed");
					return AuthenticateResult.Fail($"Authentication failed, email adress or password is incorrect");
				}
				await jwtBearerAuthenticationService.BasicAuthResponseAsync(tupleResult);
				//return AuthenticateResult.Success();
				//jwtBearerAuthenticationService.GenerateJwtToken(user);
				// var token = jwtBearerAuthenticationService.GenerateJwtToken(user);
				// redisToken.StoreTokenSessionInRedis(token, email);
				return AuthenticateResult.NoResult();
			}
		}
		catch (FormatException)
		{
			return AuthenticateResult.Fail("Invalid Authorization header encoding");
		}
		catch (Exception ex)
		{
			return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
		}
	}
}