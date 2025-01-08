using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Authentifications.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Authentifications.Services;
using Authentifications.Models;
namespace Authentifications.RedisContext;
public class RedisCacheTokenService : IRedisCacheTokenService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> logger;
	private readonly IConfiguration configuration;
	//private readonly string cacheKey = string.Empty;
	public RedisCacheTokenService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger)
	{
		_cache = cache;
		this.configuration = configuration;
		this.logger = logger;
		// this.jwtBearer = jwtBearer;
		//cacheKey = GenerateRedisKeyForTokenSession(email, password);
	}
	public bool IsTokenExpired(string token) //hangfire on check si le token est expiré dans redis
	{
		var tokenHandler = new JwtSecurityTokenHandler();
		if (tokenHandler.ReadToken(token) is not JwtSecurityToken jwtToken)
			return true;

		var expirationDate = jwtToken.ValidTo;
		return expirationDate < DateTime.UtcNow;
	}
	// public string RefreshToken(string token, string email)
	// {
	// 	if (IsTokenExpired(token))
	// 	{
	// 		return JwtBearerAuthenticationMiddleware.GenerateJwtToken(email);
	// 	}
	// 	return token;
	// }
	public string GenerateRedisKeyForTokenSession(string email, string password)
	{
		string salt = "RandomUniqueSalt";
		using SHA256 sha256 = SHA256.Create();
		string combined = $"{email}:{password}:{salt}";
		byte[] bytes = Encoding.UTF8.GetBytes(combined);
		byte[] hashBytes = sha256.ComputeHash(bytes);
		return Convert.ToHexString(hashBytes);
	}
	public string CompareRedisHashKey(string email, string password)
	{
		string salt = "RandomUniqueSalt";
		using SHA256 sha256 = SHA256.Create();
		string combined = $"{email}:{password}:{salt}";
		byte[] bytes = Encoding.UTF8.GetBytes(combined);
		byte[] hashBytes = sha256.ComputeHash(bytes);
		return BitConverter.ToString(hashBytes).Replace("-", "");
	}
	public async Task<string> RetrieveTokenBasingOnRedisUserSessionAsync(UtilisateurDto utilisateur)
	{
		if (string.IsNullOrEmpty(utilisateur.Email) || string.IsNullOrEmpty(utilisateur.Pass))
		{
			logger.LogError("Email or Password is null or empty for the user.");
			return string.Empty;
		}
		var cacheKey = CompareRedisHashKey(utilisateur.Email, utilisateur.Pass);
		var cachedData = await _cache.GetStringAsync(cacheKey);
		if (cachedData is null)
		{
			logger.LogWarning("Aucun token de session présent dans Redis  clé : {CacheKey}", cacheKey);
			return string.Empty;
		}
		var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(cachedData)!;
		string token = "";
		if (obj.ContainsKey("Token"))
			token = obj["Token"]?.ToString() ?? string.Empty;
		return token;
	 
	}
	public void StoreTokenSessionInRedis(string email, string token, string password)
	{
		Dictionary<string, object> jsonObject = new()
		{
			{ "RedisTokenId", Guid.NewGuid() },
			{ "Email", email },
			{ "Token", token }
		};
		var cacheKey = GenerateRedisKeyForTokenSession(email, password);
		var cachedData = _cache.GetStringAsync(cacheKey);
		if (cachedData is not null)
		{
			var serializedData = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
			_cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Modifier pour test
			});
			logger.LogInformation("Sauvegarde du token de connexion pour la clé: {CacheKey} réussie", cacheKey);
		}
	}
	public string RefreshToken(string token, string email)
	{
		throw new NotImplementedException();
	}
}
