using Authentifications.Models;

namespace Authentifications.Interfaces
{
	public interface IRedisCacheService
	{
		Task<ICollection<UtilisateurDto>> RetrieveData_OnRedisUsingKeyAsync();
		Task<ICollection<UtilisateurDto>> ValidateAndSyncDataAsync(string cachedData);
		Task<bool> GetDataFromRedisByFilterAsync(string email, string password);
		Task<(bool, UtilisateurDto)> GetBooleanAndUserDataFromRedisUsingParamsAsync(bool condition, string email, string password);
	}
}