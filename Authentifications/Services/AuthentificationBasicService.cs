using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using Authentifications.Interfaces;
using Authentifications.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

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
			var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);

			if (credentials.Length != 2)
				return AuthenticateResult.Fail("Invalid Authorization header format");

			var email = credentials[0];
			var password = credentials[1];
			// redisToken.GenerateRedisKeyForTokenSession(email, password);
			// redisToken.StoreTokenSessionInRedis(email);
			var user = (await redisCache.GetDataFromRedisUsingParamsAsync(true, email, password)).Item2;
			jwtBearerAuthenticationService.GenerateJwtToken(user);
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
}