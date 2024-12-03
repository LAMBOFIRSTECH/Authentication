using Authentifications.DataBaseContext;
using Authentifications.Models;

namespace Authentifications.Repositories;
public class AuthentificationBasicRepository 
{

	private readonly ApiContext context;
	public AuthentificationBasicRepository(ApiContext context)
	{
		this.context = context;
	}
	public  UtilisateurDto RetrieveCredentials(string email)
	{
		return context.GetUsersData().Where(u => u.Email == email).FirstOrDefault()!;
	}
}