# Authentifications
Pour la gestion des authentifications basique et Jwt 
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
			//combined = $"Client:{salt}";
			byte[] bytes = Encoding.UTF8.GetBytes(combined);
			byte[] hashBytes = sha256.ComputeHash(bytes);
			return Convert.ToHexString(hashBytes);
		}
	}
	public async Task<ICollection<UtilisateurDto>> StoreCredentialsAsync(string email, string password)
	{
		var baseUrl = configuration["ApiSettings:BaseUrl"];
		HttpClient httpClient = null;
		HttpResponseMessage response = null;

		try
		{
			// Créer une instance de HttpClient
			httpClient = CreateHttpClient(baseUrl);
			string cacheKey = GenerateClientId(email, password);

			// Vérifier d'abord si les données sont présentes dans le cache Redis
			var cachedData = await _cache.GetStringAsync(cacheKey);
			if (cachedData is not null)
			{
				logger.LogInformation("Données récupérées depuis Redis pour la clé : {CacheKey}", cacheKey);
				return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
			}

			try
			{
				// Si les données ne sont pas en cache, essayer de les récupérer depuis le service distant
				var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
				response = await httpClient.SendAsync(request);
				var status=  response.StatusCode;
				if (!response.IsSuccessStatusCode)
				{
					logger.LogWarning($"L'appel HTTP a échoué avec le statut : {status}. Tentative de récupération des données depuis Redis.");
					logger.LogError($"L'API a renvoyé un statut d'erreur: {response.StatusCode}");
					return new HashSet<UtilisateurDto>();  // Retourner une collection vide ou une valeur par défaut		
				}
			}
			catch (HttpRequestException ex)
			{
				// Problème de connexion ou l'API est hors ligne
				throw new Exception("L'API est hors ligne ou une erreur réseau est survenue.", ex);
			}

			// Si l'appel HTTP est réussi, on récupère les données
			var content = await response.Content.ReadAsStringAsync();
			var utilisateurs = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(content)!;
			if (utilisateurs == null)
			{
				throw new Exception("Failed to deserialize the response.");
			}

			// Sérialiser et stocker les données dans Redis pour les futures requêtes
			var serializedData = JsonConvert.SerializeObject(utilisateurs);
			await _cache.SetStringAsync(cacheKey, serializedData, new DistributedCacheEntryOptions
			{
				AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(3)
			});

			logger.LogInformation("Données mises en cache dans Redis pour la clé : {CacheKey}", cacheKey);
			return utilisateurs;
		}
		catch (Exception ex)
		{
			if (response != null)
			{
				logger.LogError("Erreur avec le statut : {Status}", response.StatusCode);
			}
			logger.LogError(ex, "Erreur lors de l'appel à l'API");

			// En cas d'erreur, tenter de récupérer les données depuis Redis
			string cacheKey = GenerateClientId(email, password);
			var cachedData = await _cache.GetStringAsync(cacheKey);

			if (cachedData != null)
			{
				logger.LogInformation("Données récupérées depuis Redis après une erreur HTTP.");
				return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
			}
			return new HashSet<UtilisateurDto>();
		}
		finally
		{
			// Si HttpClient ou HttpResponseMessage sont créés dans cette méthode, assurez-vous de les disposer après utilisation
			if (response != null)
			{
				response.Dispose(); // Libère la connexion HTTP
			}

			if (httpClient != null)
			{
				httpClient.Dispose(); // Libère les ressources liées à HttpClient
			}
		}
	}