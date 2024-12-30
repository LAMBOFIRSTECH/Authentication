using Authentifications.Models;

namespace Authentifications.Interfaces
{
	public interface IJwtToken
	{
		Task<TokenResult> GetToken(UtilisateurDto utilisateurDto);
		Task<UtilisateurDto> BasicAuthResponseAsync((bool IsValid, UtilisateurDto utilisateurDto) tupleParameter);
	}
}