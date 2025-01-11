using Authentifications.Models;

namespace Authentifications.Interfaces;
public interface IRedisCacheService
{
	Task<ICollection<UtilisateurDto>> RetrieveDataOnRedisUsingKeyAsync();
	Task<ICollection<UtilisateurDto>> ValidateAndSyncDataAsync(string cachedData);
	Task<(bool, UtilisateurDto)> GetBooleanAndUserDataFromRedisUsingParamsAsync(bool condition, string email, string password);
	Tuple<string,string> GetCredentials(string email,string password);
}