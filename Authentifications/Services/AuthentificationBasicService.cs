using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Text.Encodings.Web;
using Authentifications.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using RedisDatabase = StackExchange.Redis.IDatabase;

namespace Authentifications.Repositories;
public class AuthentificationBasicService : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly JwtBearerAuthenticationRepository jwtBearerAuthenticationRepository;
	private readonly JwtBearerAuthenticationService jwtBearerAuthenticationService;
	private readonly RedisCacheService redisCacheService;
	private readonly RedisDatabase redisDatabase;
	private readonly ILogger<RedisCacheService> log;
	public AuthentificationBasicService(RedisCacheService redisCacheService, JwtBearerAuthenticationRepository jwtBearerAuthenticationRepository, JwtBearerAuthenticationService jwtBearerAuthenticationService, IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	ISystemClock clock, ILogger<RedisCacheService> log, RedisDatabase redisDatabase)
	: base(options, logger, encoder, clock)
	{
		this.jwtBearerAuthenticationRepository = jwtBearerAuthenticationRepository;
		this.jwtBearerAuthenticationService = jwtBearerAuthenticationService;
		this.redisCacheService = redisCacheService;
		this.log = log;
		this.redisDatabase = redisDatabase;
	}
	internal async Task<bool> AuthenticateAsync(string email, string password)
	{
		string clientID = redisCacheService.GenerateClientId(email, password);
		var user = await redisCacheService.GetCredentialsAsync(clientID);
		if (user is null)
		{
			return false;
		}
		await Task.Delay(1000);
		if (!user.CheckHashPassword(password) || !user.Email!.Equals(email))
		{
			throw new KeyNotFoundException("Les infos de l'utilisateur sont invalides");
		}
		return true;
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
			string clientID = redisCacheService.GenerateClientId(email, password);
			// tester la connexion à la bd avant de save
			var isConnected = redisDatabase.Multiplexer.IsConnected;
			if (!isConnected)
			{
				throw new InvalidOperationException("Redis n'est pas connecté.");
			}
			log.LogInformation("Redis est connecté : {IsConnected}", isConnected);

			await redisCacheService.StoreCredentialsAsync(clientID, email, password);
			if (await AuthenticateAsync(email, password) == false)
			{
				return AuthenticateResult.Fail("Invalid email or password");
			}
			jwtBearerAuthenticationService.GenerateJwtToken(email);
			return AuthenticateResult.Fail("email or password incrorrect");
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