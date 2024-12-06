using System.Collections;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using Authentifications.Middlewares;
using Authentifications.Models;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;

namespace Authentifications.DataBaseContext;
public class ApiContext
{

	private readonly HttpClient _httpClient;
	private readonly IConfiguration configuration;
	public ApiContext(IConfiguration configuration, HttpClient _httpClient)
	{

		this._httpClient = _httpClient;
		this.configuration = configuration;
	}

	public async Task<List<UtilisateurDto>> GetUsersDataAsync()
	{
		var BaseUrl = configuration["ApiSettings:BaseUrl"];
		var certificateFile = configuration["Certificate:File"];
		var certificatePassword = configuration["Certificate:Password"];

		// Charger le certificat
		var certificate = new X509Certificate2(certificateFile, certificatePassword);

		// Créer un HttpClientHandler et ajouter le certificat
		var handler = new HttpClientHandler();
		handler.ClientCertificates.Add(certificate);
		handler.ServerCertificateCustomValidationCallback = (httpRequestMessage, cert, certChain, sslPolicyErrors) =>
		{
			// Accepter uniquement si la chaîne de certification est valide
			if (sslPolicyErrors == System.Net.Security.SslPolicyErrors.None)
			{
				return true;
			}

			// Loguer les erreurs pour un diagnostic logger plustard on doit le use
			Console.WriteLine($"Erreur SSL détectée : {sslPolicyErrors}");
			return false; 
		};
		using var clientWithCertificate = new HttpClient(handler)
		{
			BaseAddress = new Uri(BaseUrl)
		};

		var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");

		// Il faut trouver le moyen de fournir le certificat pour "https://localhost:7082" de TasksManagment lors de l'envoie de la requete
		var response = await _httpClient.SendAsync(request);
		response.EnsureSuccessStatusCode();
		if (response.ReasonPhrase == "No Content")
		{
			throw new Exception($"{(int)response.StatusCode}: No data to retrieve collection is empty check the former API");
		}
		var content = await response.Content.ReadAsStringAsync();
		var utilisateurs = JsonConvert.DeserializeObject<List<UtilisateurDto>>(content)!;
		return utilisateurs;
	}
}