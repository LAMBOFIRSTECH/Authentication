using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Authentifications.Interfaces;
using Authentifications.Models;
using Authentifications.Middlewares;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Authentifications.Services;
namespace Authentifications.RedisContext;
public class RedisCacheTokenService : IRedisCacheTokenService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> logger;
	private readonly IConfiguration configuration;
	private readonly JwtBearerAuthenticationService  jwtBearer;
	private readonly string cacheKey = string.Empty;
	private readonly string email = string.Empty;
	private readonly string password = string.Empty;
	public RedisCacheTokenService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger, JwtBearerAuthenticationService jwtBearer)
	{
		_cache = cache;
		this.configuration = configuration;
		this.logger = logger;
		this.jwtBearer = jwtBearer;
		cacheKey = GenerateRedisKeyForTokenSession(email, password);
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
	// public async Task<bool> GetTokenSessionFromRedisByFilterAsync(string email, string password)
	// {
	// 	bool find = false;
	// 	var utilisateurs = await RetrieveDataSession_OnRedisUsingKeyAsync();
	// 	foreach (var user in utilisateurs)
	// 	{
	// 		var result = user.CheckHashPassword(password);
	// 		if (result.Equals(true) && user.Email!.Equals(email))
	// 		{
	// 			find = true;
	// 			break;
	// 		}
	// 	}
	// 	return find;
	// }
	private async Task<ICollection<Object>> RetrieveDataSession_OnRedisUsingKeyAsync()
	{
		var cachedData = await _cache.GetStringAsync(cacheKey);
		//_cache.keys("*");
		if (cachedData is null)
		{
			logger.LogError("Aucune donnée trouvée dans Redis pour la clé : {CacheKey}", cacheKey);
			throw new Exception("Aucune donnée trouvée dans Redis");
		}
		return JsonConvert.DeserializeObject<ICollection<Object>>(cachedData)!;
	}
	public void StoreTokenSessionInRedis(string token, string email)
	{
		Dictionary<string, object> jsonObject = new()
        {
			{ "RedisTokenId", new Guid() },
			{ "Email", email },
			{ "Token", token }
		};
		var cachedData = _cache.GetStringAsync(cacheKey);
		if (cachedData is not null)
		{
			var serializedData = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
			_cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24) // Modifier pour test
			});
			logger.LogInformation("Redis a mise à jour avec les données du token de connexion pour la clé: {CacheKey}", cacheKey);
		}
	}
	public string RefreshToken(string token, string email)
	{
		throw new NotImplementedException();
	}
}
