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

	public AuthentificationBasicService(
	IRedisCacheTokenService redisToken,
	IRedisCacheService redisCache,
	JwtBearerAuthenticationService jwtBearerAuthenticationService,
	IOptionsMonitor<AuthenticationSchemeOptions> options,
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

			var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
			if (credentials.Length != 2)
				return AuthenticateResult.Fail("Invalid Authorization header format");

			var email = credentials[0];
			var password = credentials[1];

			if (!await ValidateCredentials(email, password))
			{
				log.LogWarning("Invalid credentials");
				Context.Response.StatusCode = StatusCodes.Status401Unauthorized;
				return AuthenticateResult.NoResult();
			}
			var claims = new List<Claim> { new Claim(ClaimTypes.Email, email) };

			var identity = new ClaimsIdentity(claims, Scheme.Name);
			var principal = new ClaimsPrincipal(identity);
			var ticket = new AuthenticationTicket(principal, Scheme.Name);
			return AuthenticateResult.Success(ticket);
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
	private async Task<bool> ValidateCredentials(string email, string password)
	{
		var tupleResult = await redisCache.GetDataFromRedisUsingParamsAsync(true, email, password);
		if (tupleResult.Item1 is false)
		{
			log.LogError("Authentication failed, email adress or password is incorrect");
			return false;
		}
		return true;
	}
}