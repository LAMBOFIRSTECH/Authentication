using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Authentifications.Models;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using RedisDatabase = StackExchange.Redis.IDatabase;
namespace Authentifications.Services;

public class RedisCacheService
{
	private readonly IDistributedCache _cache;
	private readonly RedisDatabase redisDatabase;
	private readonly ILogger<RedisCacheService> logger;
	public RedisCacheService(IDistributedCache cache, ILogger<RedisCacheService> logger, RedisDatabase redisDatabase)
	{
		_cache = cache;
		this.logger = logger;
		this.redisDatabase = redisDatabase;
	}
	public async Task<bool> StoreCredentialsAsync(string email, string password)
	{
		if (string.IsNullOrEmpty(email))
			throw new ArgumentException("L'email ne peut pas être nul ou vide.", nameof(email));
		if (string.IsNullOrEmpty(password))
			throw new ArgumentException("Le mot de passe ne peut pas être nul ou vide.", nameof(password));

		try
		{
			string salt = "RandomUniqueSalt"; // Peut être généré dynamiquement pour plus de sécurité
			string clientID;

			using (SHA256 sha256 = SHA256.Create())
			{
				string combined = $"{email}:{password}:{salt}";
				byte[] bytes = Encoding.UTF8.GetBytes(combined);
				byte[] hashBytes = sha256.ComputeHash(bytes);
				clientID = Convert.ToHexString(hashBytes);
			}
			UtilisateurDto user = new UtilisateurDto();
			user.clientID = clientID;
			user.Email = email;
			user.Pass = user.SetHashPassword(password);
			user.Nom = null;
			string cacheKey = $"Client:{clientID}";
			// Sérialisation et stockage dans Redis
			var jsonData = JsonSerializer.Serialize(user);
			await _cache.SetStringAsync(cacheKey, jsonData, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10) // TTL configuré
			});

			logger.LogInformation("Données enregistrées avec succès dans Redis pour la clé : {CacheKey}", cacheKey);
			return true;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Erreur lors de l'enregistrement des données pour l'email {Email}.", email);
			return false;
		}
	}

	public async Task<UtilisateurDto?> GetCredentialsAsync(string email, string password)
	{
		if (string.IsNullOrEmpty(email))
			throw new ArgumentException("L'email ne peut pas être nul ou vide.", nameof(email));
		if (string.IsNullOrEmpty(password))
			throw new ArgumentException("Le mot de passe ne peut pas être nul ou vide.", nameof(password));

		try
		{
			string salt = "RandomUniqueSalt"; // Le salt doit être identique à celui utilisé dans StoreCredentialsAsync
			string clientID;

			// Création d'un clientID basé sur un hachage SHA256 de l'email et du mot de passe
			using (SHA256 sha256 = SHA256.Create())
			{
				string combined = $"{email}:{password}:{salt}";
				byte[] bytes = Encoding.UTF8.GetBytes(combined);
				byte[] hashBytes = sha256.ComputeHash(bytes);
				clientID = Convert.ToHexString(hashBytes);
			}

			string cacheKey = $"Client:{clientID}";

			// Récupération des données de Redis
			RedisValue storedData = await redisDatabase.HashGetAsync(cacheKey,"data");
			if (string.IsNullOrEmpty(storedData))
			{
				logger.LogWarning("Aucune donnée trouvée pour la clé Redis : {CacheKey}", cacheKey);
				return null;
			}
			var utilisateur = JsonSerializer.Deserialize<UtilisateurDto>(storedData);
			logger.LogInformation("Données récupérées avec succès pour la clé Redis : {CacheKey}", cacheKey);

			return utilisateur;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Erreur lors de la récupération des données pour l'email {Email}.", email);
			return null;
		}
	}

}