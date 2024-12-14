using Authentifications.Models;

namespace Authentifications.Interfaces
{
	public interface IJwtToken
	{
		bool CheckUserSecret(string secretPass);
		Task<TokenResult> GetToken(string email,string password);
	}
}