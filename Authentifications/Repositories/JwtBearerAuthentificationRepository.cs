using Authentifications.DataBaseContext;
using Authentifications.Models;

namespace Authentifications.Repositories;
public class JwtBearerAuthentificationRepository
{
	private readonly ApiContext  context;

	public JwtBearerAuthentificationRepository(ApiContext context)
	{
		this.context = context;
	}
	public Utilisateur GenerateJwtToken(string email) // A revoir lors de la suppression du model Utilisateur
	{
		return context.Utilisateurs
		.Single(u => u.Email.ToUpper().Equals(email.ToUpper()) && u.Role.Equals(Utilisateur.Privilege.Administrateur));

	}
	public Utilisateur GetToken(string email) // A revoir lors de la suppression du model Utilisateur
	{
		return context.Utilisateurs.Where(u => u.Email.ToUpper().Equals(email.ToUpper()) && u.Role.Equals(Utilisateur.Privilege.Administrateur)).FirstOrDefault()!;

	}
}