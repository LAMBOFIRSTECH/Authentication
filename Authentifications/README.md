# Authentifications
Pour la gestion des authentifications basique et Jwt 


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
            // Si les données sont dans Redis, on les retourne
            logger.LogInformation("Données récupérées depuis Redis pour la clé : {CacheKey}", cacheKey);
            return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
        }

        // Si les données ne sont pas en cache, essayer de les récupérer depuis le service distant
        var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
        response = await httpClient.SendAsync(request);
        
        if (!response.IsSuccessStatusCode)
        {
            // Si l'appel HTTP échoue (par exemple, service down), on essaie de récupérer les données en cache.
            logger.LogWarning("L'appel HTTP a échoué avec le statut : {Status}. Tentative de récupération des données depuis Redis.");
            
            // Tentative de récupérer les données depuis Redis en cas d'échec de l'appel HTTP
            cachedData = await _cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                // Si les données sont en cache, on les retourne
                logger.LogInformation("Données récupérées depuis Redis après échec de l'appel HTTP.");
                return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
            }
            
            // Si aucune donnée n'est trouvée dans Redis, retourner une valeur par défaut ou une erreur
            logger.LogError("Aucune donnée disponible, même dans le cache Redis.");
            return new HashSet<UtilisateurDto>();  // Retourner une collection vide ou une valeur par défaut
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
            AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15)
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
        
        // Si aucune donnée n'est trouvée dans Redis, retourner une valeur par défaut ou une erreur
        return new HashSet<UtilisateurDto>();  // Retourner une collection vide ou une valeur par défaut
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


encore
using StackExchange.Redis;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;

namespace Authentifications.RedisContext
{
    public class RedisCacheService
    {
        private static readonly Dictionary<int, string> keyCache = new Dictionary<int, string>();
        private readonly IDistributedCache _cache;
        private readonly ILogger<RedisCacheService> logger;
        private readonly IConfiguration configuration;
        private readonly ConnectionMultiplexer redisConnection;
        private readonly ISubscriber redisSubscriber;

        public RedisCacheService(IConfiguration configuration, IDistributedCache cache, ILogger<RedisCacheService> logger)
        {
            _cache = cache;
            this.configuration = configuration;
            this.logger = logger;
            // Connexion Redis pour Pub/Sub
            redisConnection = ConnectionMultiplexer.Connect(configuration["Redis:ConnectionString"]);
            redisSubscriber = redisConnection.GetSubscriber();
        }

        public HttpClient CreateHttpClient(string baseUrl)
        {
            try
            {
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

        // Méthode qui stocke les informations dans Redis et les récupère
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
                    return JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData)!;
                }

                var request = new HttpRequestMessage(HttpMethod.Get, "/lambo-tasks-management/api/v1/users");
                response = await httpClient.SendAsync(request);
                response.EnsureSuccessStatusCode();

                if (response.ReasonPhrase == "No Content")
                {
                    logger.LogWarning("No data retrieved: the collection is empty.");
                    return new HashSet<UtilisateurDto>();
                }

                var content = await response.Content.ReadAsStringAsync();
                var utilisateurs = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(content)!;
                if (utilisateurs == null)
                {
                    throw new Exception("Failed to deserialize the response.");
                }

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

        // Méthode pour écouter les ajouts dans Redis (Pub/Sub)
        public void ListenForKeyEvents()
        {
            // Souscrire au canal "sadd" pour écouter les ajouts dans un SET
            redisSubscriber.Subscribe("__keyevent@0__:sadd", async (channel, message) =>
            {
                logger.LogInformation("Nouvel élément ajouté à la clé : {CacheKey}", message);
                
                // Quand un élément est ajouté dans Redis, récupérez les données
                var cachedData = await _cache.GetStringAsync(message);

                if (!string.IsNullOrEmpty(cachedData))
                {
                    // Désérialiser les données et traiter les utilisateurs
                    var utilisateurs = JsonConvert.DeserializeObject<HashSet<UtilisateurDto>>(cachedData);
                    logger.LogInformation("Données mises à jour pour la clé : {CacheKey}", message);
                    // Vous pouvez traiter les utilisateurs ici selon vos besoins
                }
            });

            logger.LogInformation("Écoute des événements Redis Pub/Sub activée pour les ajouts (SADD).");
        }

        public async Task<bool> GenerateAsyncDataByFilter(string email, string password)
        {
            bool find = false;
            var utilisateurs = await StoreCredentialsAsync(email, password);
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
    }
}






