using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Authentifications.Interfaces;
using Authentifications.Services;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
namespace Authentifications.RedisContext;
public class RedisCacheTokenService : IRedisCacheTokenService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> logger;
	private readonly IConfiguration configuration;
	private JwtBearerAuthenticationService jwtBearerAuthenticationService;
	private readonly string cacheKey = string.Empty;
	private readonly string email = string.Empty;
	private readonly string password = string.Empty;
	public RedisCacheTokenService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger, JwtBearerAuthenticationService jwtBearerAuthenticationService)
	{
		_cache = cache;
		this.configuration = configuration;
		this.logger = logger;
		this.jwtBearerAuthenticationService = jwtBearerAuthenticationService;
		cacheKey = GenerateRedisKeyForTokenSession(email, password);
	}
	public bool IsTokenExpired(string token) //hangfire on check si le token est expiré dans redis
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
			return jwtBearerAuthenticationService.GenerateJwtToken(email);
		}
		return token;
	}
	public string GenerateRedisKeyForTokenSession(string email, string password)
	{
		string salt = "RandomUniqueSalt";
		using (SHA256 sha256 = SHA256.Create())
		{
			string combined = $"{email}:{password}:{salt}";
			byte[] bytes = Encoding.UTF8.GetBytes(combined);
			byte[] hashBytes = sha256.ComputeHash(bytes);
			return Convert.ToHexString(hashBytes);
		}
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
		if (cachedData is  null)
		{
			logger.LogError("Aucune donnée trouvée dans Redis pour la clé : {CacheKey}", cacheKey);
			throw new Exception("Aucune donnée trouvée dans Redis");
		}
		return JsonConvert.DeserializeObject<ICollection<Object>>(cachedData)!;
	}
	public void StoreTokenSessionInRedis(string email)
	{
		string token = jwtBearerAuthenticationService.GenerateJwtToken(email);
		Dictionary<string, object> jsonObject = new Dictionary<string, object>
		{
			{ "RedisID", new Guid() },
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

			logger.LogInformation("Redis mise à jour avec les données de l'API pour la clé : {CacheKey}", cacheKey);

		}
	}

}
