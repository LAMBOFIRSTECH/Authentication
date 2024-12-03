using Authentifications.DataBaseContext;
using Authentifications.Models;

namespace Authentifications.Repositories;
public class JwtBearerAuthentificationRepository
{
	private readonly ApiContext context;

	public JwtBearerAuthentificationRepository(ApiContext context)
	{
		this.context = context;
	}
	// Pour les ddeux fonctions ci-dessous remplacer par une seule fonction qui accepte deux filtres differents (Fonction déléguée)
	public UtilisateurDto GetUserWithAdminPrivilege(string email) // A revoir lors de la suppression du model Utilisateur
	{
		return context.GetUsersData().Where(u => u.Email.ToUpper().Equals(email.ToUpper()) && u.Role.Equals(UtilisateurDto.Privilege.Administrateur)).FirstOrDefault()!;
	}
	public UtilisateurDto GetUserFilterByEmailAddress(string email)
	{
		return context.GetUsersData().Where(u => u.Email.ToUpper().Equals(email.ToUpper())).FirstOrDefault()!;
	}
}