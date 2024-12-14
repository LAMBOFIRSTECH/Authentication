using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using Authentifications.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Authentifications.Repositories;
public class AuthentificationBasicService : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly JwtBearerAuthenticationRepository jwtBearerAuthenticationRepository;
	private readonly JwtBearerAuthenticationService jwtBearerAuthenticationService;
	private readonly RedisCacheService redisCacheService;
	public AuthentificationBasicService(RedisCacheService redisCacheService, JwtBearerAuthenticationRepository jwtBearerAuthenticationRepository, JwtBearerAuthenticationService jwtBearerAuthenticationService, IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	ISystemClock clock)
	: base(options, logger, encoder, clock)
	{
		this.jwtBearerAuthenticationRepository = jwtBearerAuthenticationRepository;
		this.jwtBearerAuthenticationService = jwtBearerAuthenticationService;
		this.redisCacheService = redisCacheService;
	}
	internal async Task<bool> AuthenticateAsync(string email,string password)
	{
		var utilisateur = await jwtBearerAuthenticationRepository.GetUserByEmails(email,password);
		if (utilisateur is null)
		{
			return false;
		}
		await Task.Delay(1000);
		return utilisateur.CheckHashPassword(password);
	}
	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
			return AuthenticateResult.Fail("Authorization header missing");
		try
		{
			var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
			if (string.IsNullOrEmpty(authHeader.Parameter) || !authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase))
				return AuthenticateResult.Fail("Invalid Authorization header format");
			var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
			var email = credentials[0];
			var password = credentials[1];

			if (credentials.Length != 2)
				return AuthenticateResult.Fail("Invalid Authorization header format");

			// On va récupérer dans la bd redis le user en fonction de l'email
			var utilisateur = await redisCacheService.GetCredentialsAsync(email,password);
			if (utilisateur is null)
			{
				throw new ArgumentNullException(nameof(utilisateur));
			}
			if (!utilisateur.Email!.Equals(email) || !utilisateur.Pass!.Equals(utilisateur.CheckHashPassword(password)))
			{
				throw new KeyNotFoundException();
			}
			if (await AuthenticateAsync(email,password))
			{
				jwtBearerAuthenticationService.GenerateJwtToken(email,password);
			}
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