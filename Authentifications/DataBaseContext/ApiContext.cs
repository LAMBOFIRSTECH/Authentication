using System.Security.Cryptography.X509Certificates;
using Authentifications.Models;
using Newtonsoft.Json;
namespace Authentifications.DataBaseContext;
public class ApiContext
{
	private readonly HttpClient httpClient;
	private readonly IConfiguration configuration;
	private readonly ILogger<ApiContext> logger;
	private static readonly Dictionary<int, string> keyCache = new Dictionary<int, string>();
	public ApiContext(IConfiguration configuration, HttpClient httpClient, ILogger<ApiContext> logger)
	{
		this.httpClient = httpClient;
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


	public async Task<List<UtilisateurDto>> GetUsersDataAsync() // On oublie ceci pour permettre une séparation totale du service pas d'appele au service TASKSMANAGEMENT quoi que
	{
		var baseUrl = configuration["ApiSettings:BaseUrl"];
		HttpClient httpClient = CreateHttpClient(baseUrl);
		HttpResponseMessage response = null;
		try
		{
			var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
			//httpClient.BaseAddress = new Uri(baseUrl);
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
			throw;
		}
		// ParallelExecutionMode etre gérer les refresh token
		// Pratique courante dans les systèmes modernes :
		// Dans les systèmes modernes, il est fréquent de combiner les deux approches :

		// Accès direct à la base de données pour la première authentification (par exemple, lors de la connexion initiale de l'utilisateur).
		// Utilisation d'un cache (store interne) [redis] pour les authentifications répétées et pour stocker temporairement des informations comme les rôles d'utilisateur, les tokens JWT, etc.
		// Cela permet de bénéficier des avantages des deux solutions en termes de sécurité, de performance, et de scalabilité.
		// RABBIT MQ ajouter Un service d'audit pour enregistrer les tentatives de connexion.

	}
	// public async Task<UtilisateurDto?> GetUserByEmailAsync(string email)
	// {
	// 	var request = new HttpRequestMessage(HttpMethod.Get, $"/lambo-tasks-management/api/v1/users?email={email}");
	// 	var response = await _httpClient.SendAsync(request);
	// 	if (response.IsSuccessStatusCode)
	// 	{
	// 		var content = await response.Content.ReadAsStringAsync();
	// 		return JsonConvert.DeserializeObject<UtilisateurDto>(content);
	// 	}
	// 	return null;
	// }
}