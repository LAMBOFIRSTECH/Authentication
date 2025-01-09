using Authentifications.Models;

namespace Authentifications.Interfaces;
public interface IJwtAccessAndRefreshTokenService
{
    Task<TokenResult> GetToken(UtilisateurDto utilisateurDto);
    Task<UtilisateurDto> AuthUserDetailsAsync((bool IsValid, string email, string password) tupleParameter);
}