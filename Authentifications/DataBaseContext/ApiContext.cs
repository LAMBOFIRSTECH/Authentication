using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Authentifications.Middlewares;
using Authentifications.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Microsoft.Extensions.Logging;
namespace Authentifications.DataBaseContext;
public class ApiContext
{
	private readonly HttpClient _httpClient;
	private readonly IConfiguration configuration;
	private readonly ILogger<ApiContext> _logger;
	public ApiContext(IConfiguration configuration, HttpClient _httpClient, ILogger<ApiContext> logger)
	{

		this._httpClient = _httpClient;
		this.configuration = configuration;
		_logger = logger;
		var BaseUrl = configuration["ApiSettings:BaseUrl"];
		var certificateFile = configuration["Certificate:File"];
		var certificatePassword = configuration["Certificate:Password"];
		var certificate = new X509Certificate2(certificateFile, certificatePassword);
		var handler = new HttpClientHandler();
		handler.ClientCertificates.Add(certificate);
		handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, sslPolicyErrors) =>
		{
			if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
			{
				return true; 
			}

			_logger.LogError("Erreur SSL détectée : {SslErrors}", sslPolicyErrors);
			return false; // Rejeter le certificat
		};

		_httpClient = new HttpClient(handler)
		{
			BaseAddress = new Uri(BaseUrl)
		};
	}
// public async Task<UtilisateurDto?> GetUserByEmailAsync(string email)
// {
//     var request = new HttpRequestMessage(HttpMethod.Get, $"/lambo-tasks-management/api/v1/users?email={email}");
//     var response = await _httpClient.SendAsync(request);
//     if (response.IsSuccessStatusCode)
//     {
//         var content = await response.Content.ReadAsStringAsync();
//         return JsonConvert.DeserializeObject<UtilisateurDto>(content);
//     }
//     return null;
// }

	public async Task<List<UtilisateurDto>> GetUsersDataAsync() // On oublie ceci pour permettre une séparation totale du service pas d'appele au service TASKSMANAGEMENT quoi que
	{
		try
		{
		var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
		var response = await _httpClient.SendAsync(request);
		response.EnsureSuccessStatusCode();
		if (response.ReasonPhrase == "No Content")
		{
			throw new Exception($"{(int)response.StatusCode}: No data to retrieve collection is empty check the former API");
		}
		var content = await response.Content.ReadAsStringAsync();
		var utilisateurs = JsonConvert.DeserializeObject<List<UtilisateurDto>>(content)!;
		return utilisateurs;
			
		}catch (Exception ex)
		{
			  _logger.LogError(ex, "Erreur lors de l'appel à l'API");
			throw;
		}
		//ParallelExecutionMode etre gérer  les refresh token
// 		Pratique courante dans les systèmes modernes :
// Dans les systèmes modernes, il est fréquent de combiner les deux approches :

// Accès direct à la base de données pour la première authentification (par exemple, lors de la connexion initiale de l'utilisateur).
// Utilisation d'un cache (store interne) [redis] pour les authentifications répétées et pour stocker temporairement des informations comme les rôles d'utilisateur, les tokens JWT, etc.
// Cela permet de bénéficier des avantages des deux solutions en termes de sécurité, de performance, et de scalabilité.
// RABBIT MQ ajouter Un service d'audit pour enregistrer les tentatives de connexion.

	}
}