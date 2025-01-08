using Authentifications.Models;

namespace Authentifications.Interfaces;
public interface IJwtToken
{
    Task<TokenResult> GetToken(UtilisateurDto utilisateurDto);
    Task<UtilisateurDto> AuthUserDetailsAsync((bool IsValid, string email, string password) tupleParameter);
}