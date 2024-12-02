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
	public  Utilisateur IsValidCredentials(string username)
	{
		return context.Utilisateurs.Where(u => u.Nom == username).FirstOrDefault()!;
	}


}