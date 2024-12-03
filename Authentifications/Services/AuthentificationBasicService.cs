using System.Net.Http.Headers;
using System.Text;
using System.Text.Encodings.Web;
using Authentifications.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Authentifications.Repositories;
public class AuthentificationBasicService : AuthenticationHandler<AuthenticationSchemeOptions>
{
	private readonly AuthentificationBasicRepository authentificationBasic;
	private readonly JwtBearerAuthentificationService jwtBearer; // Communication interservice pb de dépendance MediatR || Gestionnaire d'évènements
	public AuthentificationBasicService(AuthentificationBasicRepository authentificationBasic,JwtBearerAuthentificationService jwtBearer, IOptionsMonitor<AuthenticationSchemeOptions> options,
	ILoggerFactory logger,
	UrlEncoder encoder,
	ISystemClock clock)
	: base(options, logger, encoder, clock)
	{
		this.authentificationBasic = authentificationBasic; 
		this.jwtBearer = jwtBearer; 
	}

	internal async Task<bool> AuthenticateAsync(string email, string password)
	{
		var utilisateur = authentificationBasic.RetrieveCredentials(email); //Trop d'appels vers la data pour la meme ressources 
		if (utilisateur != null)
		{
			return utilisateur.CheckHashPassword(password);
		}
		await Task.Delay(1000); // est ce pertinent ?
		return false;
	}
	protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
	{
		if (!Request.Headers.ContainsKey("Authorization"))
			return AuthenticateResult.Fail("Authorization header missing");

		try
		{
			var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
			if (string.IsNullOrEmpty(authHeader.Parameter) || !authHeader.Scheme.Equals("Basic", StringComparison.OrdinalIgnoreCase)) //C'etait Basic avant
				return AuthenticateResult.Fail("Invalid Authorization header format");

			var credentialBytes = Convert.FromBase64String(authHeader.Parameter!);
			var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':', 2);
			var email = credentials[0];
			var password = credentials[1];

			if (credentials.Length != 2)
				return AuthenticateResult.Fail("Invalid Authorization header format");

			if (await AuthenticateAsync(email, password))
			{
				jwtBearer.GenerateJwtToken(email);
			}
			return AuthenticateResult.Fail("password incrorrect");
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