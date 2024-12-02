namespace Authentifications.DataBaseContext;
public class ApiContext
{

	private readonly HttpClient _httpClient;
	//private readonly string _apiKey;
	public ApiContext(IHttpClientFactory httpClientFactory, IConfiguration configuration)
	{
		_httpClient = httpClientFactory.CreateClient();
		_httpClient.BaseAddress = new Uri(configuration["ApiSettings:BaseUrl"]);

		// Charger le secret depuis l'environnement ou User Secrets
		//_apiKey = configuration["ApiKey"];
		// Utilisateurs = new List<Utilisateur>
		// {
		// 	new Utilisateur { ID = Guid.NewGuid(), Nom = "Alice", Email = "alice@example.com", Role = Utilisateur.Privilege.Administrateur },
		// 	new Utilisateur { ID = Guid.NewGuid(), Nom = "Bob", Email = "bob@example.com", Role = Utilisateur.Privilege.Utilisateur },
		// 	new Utilisateur { ID = Guid.NewGuid(), Nom = "Charlie", Email = "charlie@example.com", Role = Utilisateur.Privilege.Utilisateur }
		// };
	}

 ///
	 public async Task<string> GetUsersDataAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "v1/users");
       // request.Headers.Add("Authorization", $"Bearer {_apiKey}");

        var response = await _httpClient.SendAsync(request);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}