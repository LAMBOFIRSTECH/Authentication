using Authentifications.Models;

namespace Authentifications.Interfaces
{
	public interface IRedisCacheService
	{
		Task<ICollection<UtilisateurDto>> RetrieveData_OnRedisUsingKeyOrFromExternalApiAndStoreInRedisAsync();
		Task<ICollection<UtilisateurDto>> ValidateAndSyncDataAsync(string cachedData);
		Task<bool> GetDataFromRedisByFilterAsync(string email, string password);
	}
}