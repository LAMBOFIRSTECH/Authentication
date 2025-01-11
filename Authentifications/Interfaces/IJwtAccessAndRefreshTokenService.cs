using Authentifications.Models;

namespace Authentifications.Interfaces;
public interface IJwtAccessAndRefreshTokenService
{
	abstract string GenerateRefreshToken();
	TokenResult GetToken(UtilisateurDto utilisateurDto);
	Task<UtilisateurDto> AuthUserDetailsAsync((bool IsValid, string email, string password) tupleParameter);
	Task<TokenResult>  NewAccessTokenUsingRefreshTokenAsync(string refreshToken, string email, string password);
}