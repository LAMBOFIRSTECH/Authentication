using Authentifications.Models;

namespace Authentifications.Interfaces
{
	public interface IJwtToken
	{
		Task<TokenResult> GetToken(string email);
	}
}