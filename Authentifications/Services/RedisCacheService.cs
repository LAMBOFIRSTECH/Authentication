using System.Text.Json;
using Authentifications.Models;
using Microsoft.Extensions.Caching.Distributed;
namespace Authentifications.Services;

public class RedisCacheService
{
	private readonly IDistributedCache _cache;

	public RedisCacheService(IDistributedCache cache)
	{
		_cache = cache;
	}
	public async Task StoreCredentialsAsync(string clientId, string email, string password, TimeSpan expiration)
	{
		string cacheKey = $"Client:{clientId}";
		UtilisateurDto user = new UtilisateurDto();
		user.clientID=clientId;
		user.Email = email;
		user.Pass = user.SetHashPassword(password);
		user.Nom= null;
		var jsonData = JsonSerializer.Serialize(user);
		await _cache.SetStringAsync(cacheKey, jsonData, new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = expiration
		});
	}
	public async Task<UtilisateurDto?> GetCredentialsAsync(string clientID)
	{
		var jsonData = await _cache.GetStringAsync($"Client:{clientID}");  // c'est vide ici

		if (string.IsNullOrEmpty(jsonData))
			return null;
		return JsonSerializer.Deserialize<UtilisateurDto>(jsonData);
	}
}
