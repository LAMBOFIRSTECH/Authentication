using Authentifications.Models;

namespace Authentifications.Interfaces;
public interface IRedisCacheTokenService
{
    bool IsTokenExpired(string token);
    string RefreshToken(string token, string email);
    byte[] ComputeHashUsingByte(string email, string password);
    void StoreRefreshTokenSessionInRedis(string email, string token, string password);
    Task<string> RetrieveTokenBasingOnRedisUserSessionAsync(UtilisateurDto utilisateur);
}
