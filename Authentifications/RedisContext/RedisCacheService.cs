using System.Security.Cryptography;
using System.Text;
using Authentifications.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using StackExchange.Redis;
using System.Net.Http.Headers;
using System.Net;
using System.Net.Sockets;
namespace Authentifications.RedisContext;

public class RedisCacheService
{
	private static readonly Dictionary<int, string> keyCache = new Dictionary<int, string>();
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> logger;
	private readonly IConfiguration configuration;
	private readonly IConnectionMultiplexer redisConnection;
	private readonly ISubscriber redisSubscriber;


	public RedisCacheService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger, IConnectionMultiplexer redisConnection)
	{
		_cache = cache;
		this.configuration = configuration;
		this.logger = logger;
		// Connexion Redis pour Pub/Sub
		this.redisConnection = redisConnection;
		redisSubscriber = redisConnection.GetSubscriber();
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
	public void ListenForRedisKeyEvents(string cacheKey)
	{
		// Souscrire au canal "sadd" pour écouter les ajouts dans un SET
		redisSubscriber.Subscribe(new RedisChannel("__keyevent@0__:sadd", RedisChannel.PatternMode.Literal), async (channel, message) =>
		{
			if (message.ToString() == cacheKey)
			{
				logger.LogInformation("Nouvel élément ajouté à la clé : {CacheKey}", message);

				// Quand un élément est ajouté dans Redis, récupérez les données
				var cachedData = await _cache.GetStringAsync(cacheKey);

				if (!string.IsNullOrEmpty(cachedData))
				{
					// Désérialiser les données et traiter les utilisateurs
					var utilisateurs = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData);
					logger.LogInformation("Données mises à jour pour la clé : {CacheKey}", cacheKey);
					// Vous pouvez traiter les utilisateurs ici selon vos besoins
				}
			}

		});

		logger.LogInformation("Écoute des événements Redis Pub/Sub activée pour les ajouts (SADD).");
	}
	public async Task<ICollection<UtilisateurDto>> RetrieveData_OnRedisUsingKeyOrFromExternalServiceAndStoreInRedisAsync()
	{
		var baseUrl = configuration["ApiSettings:BaseUrl"];
		HttpClient httpClient = null;
		HttpResponseMessage response = null;
		string cacheKey = null;

		httpClient = CreateHttpClient(baseUrl);
		if (cacheKey is not null)
		{
			ListenForRedisKeyEvents(cacheKey); // On pourra se passer de ça si on veut pas écouter les événements
		}
		cacheKey = GenerateRedisKey();

		// Vérifier d'abord si les données sont présentes dans le cache Redis
		var cachedData = await _cache.GetStringAsync(cacheKey);
		if (cachedData is not null)
		{
			logger.LogInformation("Données récupérées depuis Redis pour la clé : {CacheKey}", cacheKey);
			//ListenForRedisKeyEvents(cacheKey);
			return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
		}
		try
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
			response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode(); // Lui meme il lève l'exception
			var content = await response.Content.ReadAsStringAsync();
			var utilisateurs = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(content)!;
			if (utilisateurs == null)
			{
				throw new Exception("Failed to deserialize the response. Empty data retrieve from data source");
			}
			/* C'est ici qu'on compare les data
			 entre source et redis
			*/
			var result = await ValidateAndSyncDataAsync(utilisateurs); //var utilisateurs = await ValidateAndSyncDataAsync(utilisateurs);
																	   // if (result is false)
																	   // {
																	   // 	throw new Exception("Data source and Redis data are not synchronized"); On gère cette logique en bas dans la fonction
																	   // } 

			// Sérialiser et stocker les données dans Redis pour les futures requêtes
			var serializedData = JsonConvert.SerializeObject(utilisateurs);
			await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(10)
			});

			logger.LogInformation("Données mises en cache dans Redis pour la clé : {CacheKey}", cacheKey);
			return utilisateurs;
		}
		catch (HttpRequestException ex) when (ex.InnerException is SocketException socketEx)
		{
			// Gérer l'erreur de connexion réseau (service down, connexion refusée, etc.)
			logger.LogError($"Socket's problems check if TasksManagement service is UP: {socketEx.Message}");
			throw new Exception("The service in Unavailable.  Please retry soon.", ex);
		}
		catch (HttpRequestException ex)
		{
			logger.LogError($"{ex}");
			throw new Exception("Problème de communication avec l'API.", ex);
		}
		catch (Exception ex)
		{
			logger.LogError($"{ex}");
			throw new Exception("Data source service has been correctly called. However some troubles have been detected!", ex);
		}
		finally
		{
			response?.Dispose(); // Libère les ressources liées à HttpResponseMessage
			httpClient?.Dispose(); // Libère les ressources liées à HttpClient
		}
	}
	public async Task<bool> GetDataFromRedisByFilterAsync(string email, string password)
	{
		bool find = false;
		var utilisateurs = await RetrieveData_OnRedisUsingKeyOrFromExternalServiceAndStoreInRedisAsync();
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
	public async Task<bool> ValidateAndSyncDataAsync(ICollection<UtilisateurDto> utilisateurDtos)
	{
		string cacheKey = "";   // Récupérer la liste des clés dans redis
		var cachedData = await _cache.GetStringAsync(cacheKey);
		HashSet<UtilisateurDto> redisData = null;

		if (!string.IsNullOrEmpty(cachedData))
		{
			redisData = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
		}
		if (redisData == null || !redisData.SetEquals(utilisateurDtos))
		{
			// Identifier les différences
			var newData = utilisateurDtos.Except(redisData ?? new HashSet<UtilisateurDto>()).ToList();
			var removedData = (redisData ?? new HashSet<UtilisateurDto>()).Except(utilisateurDtos).ToList();
             // Additionner les deux listes
			logger.LogInformation("Nouvelles données ajoutées : {NewDataCount}", newData.Count);
			logger.LogInformation("Données supprimées : {RemovedDataCount}", removedData.Count);

			// Mettre à jour Redis avec les nouvelles données
			var serializedData = JsonConvert.SerializeObject(utilisateurDtos);
			await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
			});

			logger.LogInformation("Redis mis à jour avec les nouvelles données pour la clé : {CacheKey}", cacheKey);
		}
		else
		{
			logger.LogInformation("Les données dans Redis et dans l'API sont déjà synchronisées.");
		}
		await Task.Delay(1000);
		return false;
	}

}