using Authentifications.Models;

namespace Authentifications.DataBaseContext;
public class ApiContext
{

	//private readonly HttpClient _httpClient;
	//private readonly string _apiKey;
	public ApiContext() //
	{
		// _httpClient = httpClientFactory.CreateClient();
		// _httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);

		// Charger le secret depuis l'environnement ou User Secrets
		//_apiKey = configuration["ApiKey"];
	}

	///
	// public async Task<string> GetUsersDataAsync()
	// {
	// 	var request = new HttpRequestMessage(HttpMethod.Get, "/users");
	// 	// request.Headers.Add("Authorization", $"Bearer {_apiKey}"); Plustard

	// 	var response = await _httpClient.SendAsync(request);
	// 	response.EnsureSuccessStatusCode();

	// 	return await response.Content.ReadAsStringAsync(); désérialiser dans le Dto
	// }
	public List<UtilisateurDto> GetUsersData()
	{
		var utilisateurs = new List<UtilisateurDto>
		{
			new UtilisateurDto { ID = Guid.NewGuid(), Nom = "Alice", Email = "alice@example.com", Role = UtilisateurDto.Privilege.Administrateur,Pass="$2a$11$kIxlSXgTOxaUNuHZFck6duPdOjY1IBq/ag64xxYQO5hMj.0yzg3ma" },
			new UtilisateurDto { ID = Guid.NewGuid(), Nom = "Bob", Email = "bob@example.com", Role = UtilisateurDto.Privilege.Utilisateur,Pass="$2a$11$kIxlSXgTOxaUNuHZFck6duPdOjY1IBq/ag64xxYQO5hMj.0yzg3ma" },
			new UtilisateurDto { ID = Guid.NewGuid(), Nom = "Charlie", Email = "charlie@example.com", Role = UtilisateurDto.Privilege.Utilisateur,Pass="$2y$10$XeAX1jtJqMpO0BXjKvk5seNgZLPSpPvquu1Q2RMP4pMvrxrJ7CVD2" }
		};
		return utilisateurs;
	}
}