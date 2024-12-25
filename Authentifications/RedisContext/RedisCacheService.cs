using System.Security.Cryptography;
using System.Text;
using Authentifications.Models;
using Microsoft.Extensions.Caching.Distributed;
using System.Security.Cryptography.X509Certificates;
using Newtonsoft.Json;
namespace Authentifications.RedisContext;

public class RedisCacheService
{
	private static readonly Dictionary<int, string> keyCache = new Dictionary<int, string>();
	private readonly IDistributedCache _cache;
	private readonly ILogger<RedisCacheService> logger;
	private readonly IConfiguration configuration;

	public RedisCacheService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger)
	{
		_cache = cache;
		this.configuration = configuration;
		this.logger = logger;
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
	public string GenerateClientId(string email, string password)
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

	public async Task<ICollection<UtilisateurDto>> StoreCredentialsAsync(string email, string password)
	{
		var baseUrl = configuration["ApiSettings:BaseUrl"];
		HttpClient httpClient = CreateHttpClient(baseUrl);
		HttpResponseMessage response = null;
		try
		{
			string cacheKey = GenerateClientId(email, password);
			var cachedData = await _cache.GetStringAsync(cacheKey);

			if (cachedData is not null)
			{
				logger.LogInformation("Données récupérées depuis Redis pour la clé : {CacheKey}", cacheKey);
				return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData!)!;
			}
			var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
			response = await httpClient.SendAsync(request);
			response.EnsureSuccessStatusCode();

			if (response.ReasonPhrase == "No Content")
			{
				logger.LogWarning("No data retrieved: the collection is empty.");
				return new HashSet<UtilisateurDto>();
			}

			// Récupérer les données
			var content = await response.Content.ReadAsStringAsync();
			var utilisateurs = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(content)!;
			if (utilisateurs == null)
			{
				throw new Exception("Failed to deserialize the response.");
			}
			/* Fermer la connexion http au service lambo-tasks-management 
				Quand on ferme le service redis ne reconnait plus le cache
			*/

			// Sérialiser et stocker les données dans Redis
			var serializedData = JsonConvert.SerializeObject(utilisateurs);
			await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
			});
			logger.LogInformation("Données mises en cache dans Redis pour la clé : {CacheKey}", cacheKey);
			var users = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(serializedData);
			logger.LogInformation("Données : {serializedData}", serializedData);
			return users!;
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
	public async Task<bool> GenerateAsyncDataByFilter(string email, string password)
	{
		bool find=false;
		var utilisateurs = await StoreCredentialsAsync(email, password);
		foreach (var user in utilisateurs)
		{
			var result= user.CheckHashPassword(password);
			if (result.Equals(true) && user.Email!.Equals(email))
			{
				find = true;
				break;
			}
		}
		return find;
	}
}