using System.Security.Cryptography;
using System.Text;
using Authentifications.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.Net.Sockets;
using Authentifications.Interfaces;
using Hangfire;
using Hangfire.Redis.StackExchange;
namespace Authentifications.RedisContext;

public class RedisCacheService : IRedisCacheService
{
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> logger;
	private readonly IConfiguration configuration;
	private readonly HttpClient httpClient = null;
	private readonly HttpResponseMessage response = null; //Plustard
	private readonly string baseUrl = string.Empty;
	private readonly string cacheKey = string.Empty;
	private static DateTime _lastExecution = DateTime.MinValue;


	public RedisCacheService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger)
	{
		_cache = cache;
		this.configuration = configuration;
		this.logger = logger;
		baseUrl = configuration["ApiSettings:BaseUrl"];
		httpClient = CreateHttpClient(baseUrl);
		cacheKey = GenerateRedisKey();

	}
	public HttpClient CreateHttpClient(string baseUrl)
	{
		try
		{
			var certificateFile = configuration["Certificate:File"];
			var certificatePassword = configuration["Certificate:Password"];
			// Charger le certificat
			var certificate = new X509Certificate2(certificateFile, certificatePassword);
			// Configurer le gestionnaire HTTP
			var handler = new HttpClientHandler();
			handler.ClientCertificates.Add(certificate);
			// Validation personnalisée du certificat serveur
			handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, sslPolicyErrors) =>
			{
				if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
				{
					return true;
				}
				logger.LogError("Erreur SSL détectée : {SslErrors}", sslPolicyErrors);
				return false;
			};
			return new HttpClient(handler)
			{
				BaseAddress = new Uri(baseUrl)
			};
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Erreur lors de la création de l'HttpClient");
			throw;
		}
	}
	public string GenerateRedisKey()
	{
		string salt = "RandomUniqueSalt";
		string email = "example@example.com";
		string password = "password$1";
		using (SHA256 sha256 = SHA256.Create())
		{
			string combined = $"{email}:{password}:{salt}";
			byte[] bytes = Encoding.UTF8.GetBytes(combined);
			byte[] hashBytes = sha256.ComputeHash(bytes);
			return Convert.ToHexString(hashBytes);
		}
	}
	public async Task<bool> GetDataFromRedisByFilterAsync(string email, string password)
	{
		bool find = false;
		var utilisateurs = await RetrieveData_OnRedisUsingKeyAsync();
		foreach (var user in utilisateurs)
		{
			var result = user.CheckHashPassword(password);
			if (result.Equals(true) && user.Email!.Equals(email))
			{
				find = true;
				break;
			}
		}
		return find;
	}
	public async Task BackGroundJob() 
	{
		if ((DateTime.Now - _lastExecution).TotalMinutes >= 2)
		{
			_lastExecution = DateTime.Now;
			await RetrieveData_OnRedisUsingKeyAsync();
		}
	}
	public void DeleteRedisCacheAfterOneDay() 
	{
		if ((DateTime.Now - _lastExecution).TotalMinutes >= 5)
		{
			_lastExecution = DateTime.Now;
			_cache.Remove(cacheKey);
			logger.LogInformation("Deleting sucessfully.");
		}
	}
	public async Task<ICollection<UtilisateurDto>> RetrieveDataFromExternalApiAsync()
	{
		try
		{
			using (var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users"))
			using (var response = await httpClient.SendAsync(request))
			{
				response.EnsureSuccessStatusCode();
				var content = await response.Content.ReadAsStringAsync();
				return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(content)!;
			}		
		}
		catch (HttpRequestException ex) when (ex.InnerException is SocketException socketEx)
		{
			logger.LogError($"Socket's problems check if TasksManagement service is UP: {socketEx.Message}");
			throw new Exception("The service is unavailable. Please retry soon.", ex);
		}
		catch (Exception ex)
		{
			logger.LogError(ex, "Unexpected error while calling the API.");
			throw;
		}
		
	}
	public async Task<ICollection<UtilisateurDto>> RetrieveData_OnRedisUsingKeyAsync()
	{
		var cachedData = await _cache.GetStringAsync(cacheKey);
		if (cachedData is not null)
		{
			try
			{
				var result = await ValidateAndSyncDataAsync(cachedData);
				if (result is not null)
				{
					logger.LogInformation("Données synchronisées avec succès.");
					return result;
				}
				return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
			}
			catch (Exception ex)
			{
				logger.LogCritical("Failed to Validate data between Redis and External API Service. Erreur : {Message}", ex.Message);
				logger.LogWarning("Utilisation des données de Redis.");

				return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!; // A revoir 
			}
		}
		logger.LogInformation("Aucune données présentes dans le cache Redis.");

		var utilisateurs = await RetrieveDataFromExternalApiAsync();
		if (utilisateurs == null || !utilisateurs.Any())
		{
			throw new Exception("Failed to deserialize the response. Empty data retrieve from data source");
		}
		await UpdateRedisCacheWithExternalApiData(utilisateurs);
		return utilisateurs;
	}
#pragma warning disable CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
	public async Task<ICollection<UtilisateurDto>?> ValidateAndSyncDataAsync(string cachedData)
#pragma warning restore CS8613 // Nullability of reference types in return type doesn't match implicitly implemented member.
	{
		try
		{
			var externalApiData = (await RetrieveDataFromExternalApiAsync()).ToHashSet();

			if (externalApiData == null || !externalApiData.Any())
			{
				logger.LogWarning("L'API externe a retourné des données vides. Validation impossible.");
				return null;
			}
			var redisData = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;

			if (!redisData!.SetEquals(externalApiData))
			{
				logger.LogInformation("Synchronisation des données.........");
				await UpdateRedisCacheWithExternalApiData(externalApiData);
				return externalApiData;
			}

			logger.LogInformation("Les données Redis sont identiques à celles de l'API. Utilisation des données de Redis.");
			return redisData;
		}
		catch (HttpRequestException ex) when (ex.InnerException is SocketException)
		{
			logger.LogWarning("Impossible de valider les données avec l'API externe. L'API est inaccessible.");
			return null;
		}
		catch (Exception ex)
		{
			logger.LogError("Une erreur s'est produite lors de la validation des données : {Message}", ex.Message);
			throw;
		}
	}
	private async Task UpdateRedisCacheWithExternalApiData(ICollection<UtilisateurDto> externalApiData)
	{
		// Sérialiser et stocker les données dans Redis
		var serializedData = JsonConvert.SerializeObject(externalApiData);
		await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
		{
			AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
		});

		logger.LogInformation("Redis mise à jour avec les données de l'API pour la clé : {CacheKey}", cacheKey);
	}
}