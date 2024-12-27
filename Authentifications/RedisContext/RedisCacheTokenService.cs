using Microsoft.Extensions.Caching.Distributed;
namespace Authentifications.RedisContext;
public class RedisCacheTokenService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> logger;
	private readonly IConfiguration configuration;
	public RedisCacheTokenService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger)
	{
		_cache = cache;
		this.configuration = configuration;
		this.logger = logger;
	}
}
