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
	public string GenerateClientId(string email, string password)
	{
		string salt = "RandomUniqueSalt"; // Peut être remplacé par un sel dynamique sécurisé
		using (SHA256 sha256 = SHA256.Create())
		{
			string combined = $"{email}:{password}:{salt}";
			byte[] bytes = Encoding.UTF8.GetBytes(combined);
			byte[] hashBytes = sha256.ComputeHash(bytes);
			return Convert.ToHexString(hashBytes);
		}
	}

	public async Task<bool> StoreCredentialsAsync(string clientID, string email, string password)
	{
		if (string.IsNullOrEmpty(clientID))
			throw new ArgumentException("Le clientID ne peut pas être nul ou vide.", nameof(clientID));
		if (string.IsNullOrEmpty(email))
			throw new ArgumentException("L'email ne peut pas être nul ou vide.", nameof(email));
		if (string.IsNullOrEmpty(password))
			throw new ArgumentException("Le mot de passe ne peut pas être nul ou vide.", nameof(password));

		try
		{
			UtilisateurDto user = new UtilisateurDto();
			user.clientID = clientID;
			user.Email = email;
			user.Pass = user.SetHashPassword(password);
			user.Nom = null;
			string cacheKey = $"Client:{clientID}";
			// Sérialisation et stockage dans Redis
			var serializedUser = JsonSerializer.Serialize(user);
			await redisDatabase.HashSetAsync(cacheKey, new HashEntry[] { new HashEntry("data", serializedUser) });

			// Enregistrez également une référence basée sur l'email
			string emailCacheKey = $"Email:{email}";
			await redisDatabase.HashSetAsync(emailCacheKey, new HashEntry[] { new HashEntry("data", serializedUser) });

			// Facultatif : définissez une durée d'expiration
			await redisDatabase.KeyExpireAsync(cacheKey, TimeSpan.FromHours(1));
			await redisDatabase.KeyExpireAsync(emailCacheKey, TimeSpan.FromHours(1));

			logger.LogInformation("Données enregistrées avec succès dans Redis pour la clé : {CacheKey}", cacheKey);
			return true;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Erreur lors de l'enregistrement des données pour l'email {Email}.", email);
			return false;
		}
	}
	public async Task<UtilisateurDto?> GetCredentialsAsync(string email = null, string password=null)
	{
		// Vérifiez que l'un des deux critères est fourni
		if ( string.IsNullOrEmpty(email))
			throw new ArgumentException("Le clientID ou l'email doit être fourni.");

		try
		{
			// var clientID = GenerateClientId(email, password);
			string cacheKey =$"Email:{email}";
			// string cacheKey = !string.IsNullOrEmpty(clientID)
			// 	? $"Client:{clientID}"
			// 	: $"Email:{email}";

			// Récupérez les données depuis Redis
			var storedData = await redisDatabase.HashGetAsync(cacheKey, "data");
			if (storedData.IsNullOrEmpty)
			{
				logger.LogWarning("Aucune donnée trouvée pour la clé Redis : {CacheKey}", cacheKey); // Ici le matter
				return null;
			}

			// Désérialisez les données en objet
			var utilisateur = JsonSerializer.Deserialize<UtilisateurDto>(storedData);
			logger.LogInformation("Données récupérées avec succès pour la clé Redis : {CacheKey}", cacheKey);

			return utilisateur;
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Erreur lors de la récupération des données pour clientID ou email.");
			return null;
		}
	}


}