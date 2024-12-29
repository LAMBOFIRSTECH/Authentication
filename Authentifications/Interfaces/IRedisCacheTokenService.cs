using Authentifications.Models;

namespace Authentifications.Interfaces
{
	public interface IRedisCacheTokenService
	{
		bool IsTokenExpired(string token);
		string RefreshToken(string token, string email);
		string GenerateRedisKeyForTokenSession(string email,string password);
		void StoreTokenSessionInRedis(string email);
	}
}