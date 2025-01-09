using System.IdentityModel.Tokens.Jwt;
using System.Net.Sockets;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentifications.Interfaces;
using Authentifications.Models;
using Microsoft.IdentityModel.Tokens;
using VaultSharp;
using VaultSharp.V1.AuthMethods.Token;
namespace Authentifications.Services;
public class JwtAccessAndRefreshTokenService : IJwtAccessAndRefreshTokenService
{
	private RsaSecurityKey rsaSecurityKey;
	private readonly IConfiguration configuration;
	private readonly ILogger<RsaSecurityKey> log;
	private readonly IRedisCacheService redisCache;
	private readonly IRedisCacheTokenService redisTokenCache;
	public JwtAccessAndRefreshTokenService(IConfiguration configuration, ILogger<RsaSecurityKey> log, IRedisCacheService redisCache, IRedisCacheTokenService redisTokenCache)
	{
		this.configuration = configuration;
		this.log = log;
		this.redisCache = redisCache;
		this.redisTokenCache = redisTokenCache;
		rsaSecurityKey = GetOrCreateSigningKey();
	}
	public async Task<TokenResult> GetToken(UtilisateurDto utilisateurDto)
	{
		//var tokenResult = new TokenResult();
		// var value = await redisTokenCache.RetrieveTokenBasingOnRedisUserSessionAsync(utilisateurDto);
		// if (!string.IsNullOrWhiteSpace(value))
		// {
		// 	tokenResult.Response = true; tokenResult.Token = value; tokenResult.RefreshToken = null;
		// 	return tokenResult;
		// }
		log.LogInformation("Création du token de session pour l'utilisateur : {utilisateur.Email}", utilisateurDto.Email);
		var result = GenerateJwtTokenAndStatefulRefreshToken(utilisateurDto);
		result.Response = true;
		await Task.Delay(500);
		redisTokenCache.StoreRefreshTokenSessionInRedis(utilisateurDto.Email!, result.RefreshToken!, utilisateurDto.Pass!);
		return result;
	}
	private RsaSecurityKey GetOrCreateSigningKey()
	{
		if (rsaSecurityKey != null)
			return rsaSecurityKey;
		// Génération de la clé RSA
		var rsa = RSA.Create(2048);
		// Exporter la clé publique en Base64 pour Vault
		_ = ConvertToPem(rsa.ExportRSAPrivateKey(), "RSA PRIVATE KEY");
		var publicKey = ConvertToPem(rsa.ExportRSAPublicKey(), "RSA PUBLIC KEY");
		StorePublicKeyInVault(publicKey);
		// Exporter la clé privée pour la signature
		rsaSecurityKey = new RsaSecurityKey(rsa.ExportParameters(true));
		return rsaSecurityKey;
	}
	private static string ConvertToPem(byte[] keyBytes, string keyType)
	{
		var base64Key = Convert.ToBase64String(keyBytes);
		var sb = new StringBuilder();
		sb.AppendLine($"-----BEGIN {keyType}-----");
		int lineLength = 64;
		for (int i = 0; i < base64Key.Length; i += lineLength)
		{
			sb.AppendLine(base64Key.Substring(i, Math.Min(lineLength, base64Key.Length - i)));
		}
		sb.AppendLine($"-----END {keyType}-----");
		return sb.ToString();
	}
	private void StorePublicKeyInVault(string publicKeyPem)
	{
		// Configuration du client HashiCorp Vault
		var hashiCorpToken = configuration["HashiCorp:VaultToken"];
		var hashiCorpHttpClient = configuration["HashiCorp:HttpClient:BaseAddress"];
		if (string.IsNullOrEmpty(hashiCorpToken) || string.IsNullOrEmpty(hashiCorpHttpClient))
		{
			log.LogWarning("La configuration de HashiCorp Vault est manquante ou invalide.");
			throw new InvalidOperationException("La configuration de HashiCorp Vault est manquante ou invalide.");
		}
		var authMethod = new TokenAuthMethodInfo(hashiCorpToken);
		var vaultClientSettings = new VaultClientSettings($"{hashiCorpHttpClient}", authMethod);
		var vaultClient = new VaultClient(vaultClientSettings);
		try
		{
			var secretPath = configuration["HashiCorp:SecretsPath"];
			vaultClient.V1.Secrets.KeyValue.V2.WriteSecretAsync(secretPath, new Dictionary<string, object>
		{
			{ "authenticationPublicKey", publicKeyPem }
		}).Wait();

			log.LogInformation("Successfull storage public key Vault !");
		}
		catch (Exception ex) when (ex.InnerException is SocketException socket)
		{
			log.LogError("Socket's problems check if TasksManagement service is UP: {socket.Message}", socket.Message);
			throw new Exception("The service is unavailable. Please retry soon.", ex);
		}
	}
	public TokenResult GenerateJwtTokenAndStatefulRefreshToken(UtilisateurDto utilisateurDto)
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		var additionalAudiences = new[] { "https://localhost:7082", "https://audience2.com", "https://localhost:9500", "https://192.168.153.131:7250", "https://audience1.com" };
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[] {
					new Claim(ClaimTypes.Name, utilisateurDto.Nom),
					new Claim(ClaimTypes.Email, utilisateurDto.Email!),
					new Claim(ClaimTypes.Role, utilisateurDto.Role.ToString()),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
				}
			),
			Expires = DateTime.UtcNow.AddMinutes(5),
			SigningCredentials = new SigningCredentials(GetOrCreateSigningKey(), SecurityAlgorithms.RsaSha512),
			Issuer = configuration.GetSection("JwtSettings")["Issuer"],
			Audience = null,
			Claims = new Dictionary<string, object>
		{
			{ JwtRegisteredClaimNames.Aud, additionalAudiences }
		}
		};
		var tokenCreation = tokenHandler.CreateToken(tokenDescriptor);
		var token = tokenHandler.WriteToken(tokenCreation);

		var refreshTokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[]
			{
				new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
			}),
			Expires = DateTime.UtcNow.AddHours(1), // Refresh token valide 7 jours normalement ou plus
			SigningCredentials = new SigningCredentials(GetOrCreateSigningKey(), SecurityAlgorithms.RsaSha512),
			Issuer = configuration.GetSection("JwtSettings")["Issuer"]
		};

		var refreshToken = tokenHandler.WriteToken(tokenHandler.CreateToken(refreshTokenDescriptor));
		TokenResult result = new()
		{
			Token = token,
			RefreshToken = refreshToken
		};

		return result;
	}
	public async Task<UtilisateurDto> AuthUserDetailsAsync((bool IsValid, string email, string password) tupleParameter)
	{
		await Task.Delay(50);
		var Parameter = await redisCache.GetBooleanAndUserDataFromRedisUsingParamsAsync(tupleParameter.IsValid, tupleParameter.email!, tupleParameter.password!);
		log.LogInformation("Authentication successful {Parameter.Item1}", Parameter.Item1);
		return Parameter.Item2;
	}
	public async Task<bool> CheckExistedJwtRefreshToken(string refreshToken,string email,string password)
	{
		//var tokenResult = new TokenResult();
		// var value = await redisTokenCache.RetrieveTokenBasingOnRedisUserSessionAsync(utilisateurDto);
		// if (!string.IsNullOrWhiteSpace(value))
		// {
		// 	tokenResult.Response = true; tokenResult.Token = value; tokenResult.RefreshToken = null;
		// 	return tokenResult;
		// }
		await Task.Delay(50);
		return false;
	}
}