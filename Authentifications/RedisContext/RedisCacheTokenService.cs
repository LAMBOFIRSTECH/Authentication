using System.IdentityModel.Tokens.Jwt;
using System.Security.Cryptography;
using System.Text;
using Authentifications.Interfaces;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using Authentifications.Services;
using Authentifications.Models;
using System.Text.RegularExpressions;
namespace Authentifications.RedisContext;
public class RedisCacheTokenService : IRedisCacheTokenService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<RedisCacheService> logger;
    private readonly IConfiguration configuration;
    public RedisCacheTokenService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger)
    {
        _cache = cache;
        this.configuration = configuration;
        this.logger = logger;
        // this.jwtBearer = jwtBearer;
    }
    public bool IsTokenExpired(string token) //hangfire on check si le token a expiré dans redis
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        if (tokenHandler.ReadToken(token) is not JwtSecurityToken jwtToken)
            return true;

        var expirationDate = jwtToken.ValidTo;
        return expirationDate < DateTime.UtcNow;
    }
    // public string RefreshToken(string token, string email) // dans TasksManagement
    // {
    // 	if (IsTokenExpired(token))
    // 	{
    // 		return JwtBearerAuthenticationMiddleware.GenerateJwtToken(email);
    // 	}
    // 	return token;
    // }

    public byte[] ComputeHashUsingByte(string email, string password)
    {
        string salt = "RandomUniqueSalt";
        using SHA256 sha256 = SHA256.Create();
        string combined = $"{email}:{password}:{salt}";
        byte[] bytes = Encoding.UTF8.GetBytes(combined);
        return sha256.ComputeHash(bytes);
    }
    public async Task<string> RetrieveTokenBasingOnRedisUserSessionAsync(UtilisateurDto utilisateur)
    {
        if (string.IsNullOrEmpty(utilisateur.Email) || string.IsNullOrEmpty(utilisateur.Pass))
            return string.Empty;

        var tampon = BitConverter.ToString(ComputeHashUsingByte(utilisateur.Email, utilisateur.Pass)).Replace("-", "");
        var name = Regex.Match(utilisateur.Email, "^[^@]+");
        var cacheKey = $"Token-{name}_{tampon}";
        var cachedData = await _cache.GetStringAsync(cacheKey);
        if (cachedData is null)
        {
            logger.LogWarning("No session's token existing in Redis for key: {CacheKey}", cacheKey);
            return string.Empty;
        }
        var obj = JsonConvert.DeserializeObject<Dictionary<string, object>>(cachedData)!;
        string token = "";
        if (obj.ContainsKey("refreshToken"))
            token = obj["refreshToken"]?.ToString() ?? string.Empty;
        return token;
    }
    public void StoreRefreshTokenSessionInRedis(string email, string refreshToken, string password)
    {
        Dictionary<string, object> jsonObject = new()
        {
            { "RedisRefreshTokenId", Guid.NewGuid() },
            { "Email", email },
            { "refreshToken", refreshToken }
        };
        var tampon = Convert.ToHexString(ComputeHashUsingByte(email, password));
        var name = Regex.Match(email, "^[^@]+");
        var cacheKey = $"Token-{name}_{tampon}";
        var cachedData = _cache.GetStringAsync(cacheKey);
        if (cachedData is not null)
        {
            var serializedData = JsonConvert.SerializeObject(jsonObject, Formatting.Indented);
            _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
            });
            logger.LogInformation("Successfull storage refresh token connection for key: {CacheKey}", cacheKey);
        }
    }
    public string RefreshToken(string token, string email)
    {
        throw new NotImplementedException();
    }
}
