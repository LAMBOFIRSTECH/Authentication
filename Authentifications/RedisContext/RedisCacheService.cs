using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Authentifications.Models;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using RedisDatabase = StackExchange.Redis.IDatabase;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
using System.Net;
namespace Authentifications.RedisContext;

public class RedisCacheService
{
	private static readonly Dictionary<int, string> keyCache = new Dictionary<int, string>();
	private readonly IDistributedCache _cache;
	private readonly RedisDatabase redisDatabase;
	private readonly ILogger<RedisCacheService> logger;
	//private readonly HttpClient httpClient;
	private readonly string clientID;
	private readonly IConfiguration configuration;

	public RedisCacheService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger, RedisDatabase redisDatabase)
	{
		_cache = cache;
		//this.httpClient = httpClient;
		this.configuration = configuration;
		this.logger = logger;
		this.redisDatabase = redisDatabase;
		clientID = GenerateClientId("");
	}
	// public RedisCacheService(IConfiguration configuration, HttpClient httpClient, ILogger<HttpClient> logger)
	// {
	// 	this.httpClient = httpClient;
	// 	this.configuration = configuration;
	// 	this.logger = logger;

	// }
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
	public async Task<List<UtilisateurDto>> GetUsersDataAsync()
	{
		var baseUrl = configuration["ApiSettings:BaseUrl"];
		HttpClient httpClient = CreateHttpClient(baseUrl);
		HttpResponseMessage response = null;
		try
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
			response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();
			if (response.ReasonPhrase == "No Content")
			{
				logger.LogWarning("No data retrieved: the collection is empty.");
				return new List<UtilisateurDto>();
			}
			var content = await response.Content.ReadAsStringAsync();
			var utilisateurs = JsonConvert.DeserializeObject<List<UtilisateurDto>>(content)!;
			if (utilisateurs == null)
			{
				throw new Exception("Failed to deserialize the response.");
			}
			return utilisateurs;
		}
		catch (Exception ex)
		{
			if (response != null)
			{
				logger.LogError("Erreur avec le statut : {Status}", response.StatusCode);
			}
			logger.LogError(ex, "Erreur lors de l'appel à l'API");
			throw;
		}
	}
	public string GenerateClientId(string credential)
	{
		string salt = "RandomUniqueSalt";
		using (SHA256 sha256 = SHA256.Create())
		{
			string combined = $"{credential}:{salt}";
			byte[] bytes = Encoding.UTF8.GetBytes(combined);
			byte[] hashBytes = sha256.ComputeHash(bytes);
			return Convert.ToHexString(hashBytes);
		}
	}
	// public async Task<List<UtilisateurDto>> StoreCredentialsAsync()
	// {
	/* Ouvrir la connexion à l'autre microservice et penser à la fermer après avoir récupérer les data 
		Récupération et désérialisation de la liste des users dans UtilisateurDto
		Stockage dans redis 
		Mise à jour du redis cli pour pull le service d'expo des data 
		Construction du docker-compose pour redis et mise en évidence du data persistance
	
	*/
	// 	var baseUrl = configuration["ApiSettings:BaseUrl"];
	// 	HttpClient httpClient = CreateHttpClient(baseUrl);
	// 	HttpResponseMessage response = null;
	// 	try
	// 	{
	// 		// Redis cache key
	// 		string cacheKey = $"Client:{clientID}";

	// 		// Vérifier si les données sont déjà dans Redis
	// 		var cachedData = await redisDatabase.StringGetAsync(cacheKey);
	// 		if (!cachedData.IsNullOrEmpty)
	// 		{
	// 			logger.LogInformation("Données récupérées depuis Redis pour la clé : {CacheKey}", cacheKey);
	// 			return JsonConvert.DeserializeObject<List<UtilisateurDto>>(cachedData)!;
	// 		}

	// 		// Effectuer l'appel API si les données ne sont pas en cache
	// 		var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
	// 		response = await httpClient.SendAsync(request);
	// 		response.EnsureSuccessStatusCode();

	// 		if (response.ReasonPhrase == "No Content")
	// 		{
	// 			logger.LogWarning("No data retrieved: the collection is empty.");
	// 			return new List<UtilisateurDto>();
	// 		}

	// 		// Récupérer les données
	// 		var content = await response.Content.ReadAsStringAsync();
	// 		var utilisateurs = JsonConvert.DeserializeObject<List<UtilisateurDto>>(content)!;
	// 		if (utilisateurs == null)
	// 		{
	// 			throw new Exception("Failed to deserialize the response.");
	// 		}

	// 		// Sérialiser et stocker les données dans Redis
	// 		var serializedData = JsonConvert.SerializeObject(utilisateurs);
	// 		await redisDatabase.StringSetAsync(cacheKey, serializedData, TimeSpan.FromHours(1));

	// 		logger.LogInformation("Données mises en cache dans Redis pour la clé : {CacheKey}", cacheKey);

	// 		return utilisateurs;
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		if (response != null)
	// 		{
	// 			logger.LogError("Erreur avec le statut : {Status}", response.StatusCode);
	// 		}
	// 		logger.LogError(ex, "Erreur lors de l'appel à l'API");
	// 		throw;
	// 	}
	// }


	// public async Task<bool> StoreCredentialsAsync(string clientID, string email, string password)
	// {
	// 	if (string.IsNullOrEmpty(clientID))
	// 		throw new ArgumentException("Le clientID ne peut pas être nul ou vide.", nameof(clientID));
	// 	if (string.IsNullOrEmpty(email))
	// 		throw new ArgumentException("L'email ne peut pas être nul ou vide.", nameof(email));
	// 	if (string.IsNullOrEmpty(password))
	// 		throw new ArgumentException("Le mot de passe ne peut pas être nul ou vide.", nameof(password));

	// 	try
	// 	{
	// 		UtilisateurDto user = new UtilisateurDto();
	// 		user.clientID = clientID;
	// 		user.Email = email;
	// 		user.Pass = user.SetHashPassword(password);
	// 		user.Nom = null;
	// 		string cacheKey = $"Client:{clientID}";
	// 		// Sérialisation et stockage dans Redis
	// 		var serializedUser = System.Text.Json.JsonSerializer.Serialize(user);
	// 		await redisDatabase.HashSetAsync(cacheKey, new HashEntry[] { new HashEntry("data", serializedUser) });
	// 		// Enregistrez également une référence basée sur l'email
	// 		string emailCacheKey = $"Email:{email}";
	// 		await redisDatabase.HashSetAsync(emailCacheKey, new HashEntry[] { new HashEntry("data", serializedUser) });
	// 		// Facultatif : définissez une durée d'expiration
	// 		await redisDatabase.KeyExpireAsync(cacheKey, TimeSpan.FromHours(1));
	// 		await redisDatabase.KeyExpireAsync(emailCacheKey, TimeSpan.FromHours(1));

	// 		logger.LogInformation("Données enregistrées avec succès dans Redis pour la clé : {CacheKey}", cacheKey);
	// 		return true;
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		logger.LogError(ex, "Erreur lors de l'enregistrement des données pour l'email {Email}.", email);
	// 		return false;
	// 	}
	// }
	// public async Task<UtilisateurDto?> GetCredentialsAsync(string email = null, string password = null)
	// {
	// 	if (string.IsNullOrEmpty(email))
	// 		throw new ArgumentException("Le clientID ou l'email doit être fourni.");

	// 	try
	// 	{
	// 		// var clientID = GenerateClientId(email, password);
	// 		string cacheKey = $"Email:{email}";
	// 		// string cacheKey = !string.IsNullOrEmpty(clientID)
	// 		// 	? $"Client:{clientID}"
	// 		// 	: $"Email:{email}";

	// 		// Récupérez les données depuis Redis
	// 		var storedData = await redisDatabase.HashGetAsync(cacheKey, "data");
	// 		if (storedData.IsNullOrEmpty)
	// 		{
	// 			logger.LogWarning("Aucune donnée trouvée pour la clé Redis : {CacheKey}", cacheKey); // Ici le matter
	// 			return null;
	// 		}
	// 		// Désérialisez les données en objet
	// 		var utilisateur = System.Text.Json.JsonSerializer.Deserialize<UtilisateurDto>(storedData);
	// 		logger.LogInformation("Données récupérées avec succès pour la clé Redis : {CacheKey}", cacheKey);

	// 		return utilisateur;
	// 	}
	// 	catch (Exception ex)
	// 	{
	// 		logger.LogError(ex, "Erreur lors de la récupération des données pour clientID ou email.");
	// 		return null;
	// 	}
	// }
}