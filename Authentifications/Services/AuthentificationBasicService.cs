using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using Authentifications.Interfaces;
using Authentifications.Models;
using Authentifications.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using static Authentifications.Models.UtilisateurDto;

namespace Authentifications.Services;
public class AuthentificationBasicService : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly JwtBearerAuthenticationService jwtBearerAuthenticationService;
	private readonly ILogger<JwtBearerAuthenticationService> log;
	private readonly IRedisCacheTokenService redisToken;
	private readonly IRedisCacheService redisCache;
	private readonly RequestDelegate _next;

	public AuthentificationBasicService(IRedisCacheTokenService redisToken, IRedisCacheService redisCache, JwtBearerAuthenticationService jwtBearerAuthenticationService, IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	ISystemClock clock, ILogger<JwtBearerAuthenticationService> log, RequestDelegate next)
	
	: base(options, logger, encoder, clock)
	{
		this.jwtBearerAuthenticationService = jwtBearerAuthenticationService;
		this.log = log;
		this.redisToken = redisToken;
		this.redisCache = redisCache;
		this._next = next;
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
			// if (Request.ContentType == null || !Request.ContentType.Contains("application/json"))
			// 	return AuthenticateResult.Fail("Invalid Content-Type. Expected 'application/json'.");

			var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
			if (credentials.Length != 2)
				return AuthenticateResult.Fail("Invalid Authorization header format");

			var email = credentials[0];
			var password = credentials[1];
			

			if (ValidateCredentials(email, password))
			{
				// Créer un ticket d'authentification si les informations sont valides
				var claims = new List<Claim>
			{
				new Claim(ClaimTypes.Email, email) // Le nom de l'utilisateur
			};

				var identity = new ClaimsIdentity(claims, "Basic");
				var principal = new ClaimsPrincipal(identity);

				// Ajouter le principal au HttpContext
				Context.User = principal;

				// Passer au middleware suivant
				await _next(Context);
			}
			else
			{
				log.LogWarning("Invalid credentials");
				Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
			}

			//Avant meme de générer un token se ressurer qu'il est présent dans redis et qu'il n'a pas été révoqué avant (d'ou la blacklist des sessions de token revoqué dans redis)
			//On peut aussi ajouter un champ dans la base de données pour savoir si le token est révoqué ou pas
			// redisToken.GenerateRedisKeyForTokenSession(email, password);
			// redisToken.StoreTokenSessionInRedis(email);
			var tupleResult = await redisCache.GetDataFromRedisUsingParamsAsync(true, email, password);
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
		catch (FormatException)
		{
			return AuthenticateResult.Fail("Invalid Authorization header encoding");
		}
		catch (Exception ex)
		{
			return AuthenticateResult.Fail($"Authentication failed: {ex.Message}");
		}
	}
	private bool ValidateCredentials(string username, string password)
	{
		// Validation fictive des informations d'identification
		// Remplacez cela par une vérification réelle (par exemple, une base de données)
		return username == "lambo@example.com" && password == "lambo";
	}
}