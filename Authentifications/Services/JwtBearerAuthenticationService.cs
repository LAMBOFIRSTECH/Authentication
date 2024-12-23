using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using Authentifications.Interfaces;
using Authentifications.Models;
using Microsoft.IdentityModel.Tokens;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
namespace Authentifications.Services;
public class JwtBearerAuthenticationService : IJwtToken
{
	private readonly IConfiguration configuration;
	private RsaSecurityKey rsaSecurityKey;
	private readonly ILogger<RsaSecurityKey> log;
	public JwtBearerAuthenticationService(IConfiguration configuration,ILogger<RsaSecurityKey> log)
	{
		this.configuration = configuration;
		this.log = log;
		rsaSecurityKey = GetOrCreateSigningKey();
	}
	public async Task<TokenResult> GetToken(string email)
	{
		await Task.Delay(500);
		return new TokenResult
		{
			Response = true,
			Token = GenerateJwtToken(email)
		};
	}
	private RsaSecurityKey GetOrCreateSigningKey()
	{
		if (rsaSecurityKey != null)
			return rsaSecurityKey;

		// Génération de la clé RSA
		using (var rsa = RSA.Create(2048))
		{
			// Exporter la clé publique en Base64 pour Vault
			var publicKey = rsa.ExportRSAPublicKey();
			string publicKeyPem = Convert.ToBase64String(publicKey);
			log.LogInformation(publicKeyPem);
			// Stocker la clé publique dans HashiCorp Vault
			// StorePublicKeyInVault(publicKeyPem);
			// Exporter la clé privée pour la signature
			return new RsaSecurityKey(rsa.ExportParameters(true));
		}
	}
	// private void StorePublicKeyInVault(string publicKeyPem)
	// {
	// 	// Configuration du client HashiCorp Vault
	// 	var hashiCorpToken = configuration["HashiCorp:VaultToken"];
	// 	var hashiCorpHttpClient = configuration["HashiCorp:HttpClient"];
	// 	var authMethod = new TokenAuthMethodInfo(hashiCorpToken);
	// 	var vaultClientSettings = new VaultClientSettings($"{hashiCorpHttpClient}", authMethod);
	// 	var vaultClient = new VaultClient(vaultClientSettings);

	// 	// Chemin pour stocker la clé publique dans hashicorp
	// 	var secretPath = "keys/rsa-public";
	// 	// Stocker la clé publique
	// 	vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(secretPath, new Dictionary<string, object>
	// 	{
	// 		{ "key", publicKeyPem }
	// 	}).Wait();

	// 	log.LogInformation("Clé publique stockée avec succès dans Vault !");
	// }

	public string GenerateJwtToken(string email)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[] {
					new Claim(ClaimTypes.Email, email),
					new Claim(ClaimTypes.Role, LoginRequest.Privilege.Administrateur.ToString()),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
				}
			),
			Expires = DateTime.UtcNow.AddHours(1),
			SigningCredentials = new SigningCredentials(rsaSecurityKey, SecurityAlgorithms.HmacSha512Signature),
			Issuer = configuration.GetSection("JwtSettings")["Issuer"],
			Audience = "https://audience1.com" // Primary audience
		};
		var additionalAudiences = new[] { "https://audience2.com", "https://localhost:9500", "https://localhost:7082", "https://192.168.153.131:7250" }; // Notre API et potentiellement le broker MQ
		tokenDescriptor.AdditionalHeaderClaims = new Dictionary<string, object>
		{
			{ JwtRegisteredClaimNames.Aud, additionalAudiences }
		};
		var tokenCreation = tokenHandler.CreateToken(tokenDescriptor);
		var token = tokenHandler.WriteToken(tokenCreation);
		return token;
	}
	public bool IsTokenExpired(string token)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        var jwtToken = tokenHandler.ReadToken(token) as JwtSecurityToken;
        if (jwtToken == null)
            return true;

        var expirationDate = jwtToken.ValidTo;
        return expirationDate < DateTime.UtcNow;
    }

    public string RefreshToken(string token, string email)
    {
        if (IsTokenExpired(token))
        {
            return GenerateJwtToken(email);
        }
        return token;
    }
}