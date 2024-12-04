using Authentifications.DataBaseContext;
using Authentifications.Models;
using Microsoft.EntityFrameworkCore;

namespace Authentifications.Repositories;
public class JwtBearerAuthentificationRepository
{
	private readonly ApiContext context;

	public JwtBearerAuthentificationRepository(ApiContext context)
	{
		this.context = context;
	}
	// Pour les deux fonctions ci-dessous remplacer par une seule fonction qui accepte deux filtres differents (Fonction déléguée)
	public UtilisateurDto GetUserWithAdminPrivilege(string email) // A revoir lors de la suppression du model Utilisateur
	{
		return context.GetUsersData().Where(u => u.Email.ToUpper().Equals(email.ToUpper()) && u.Role.Equals(UtilisateurDto.Privilege.Administrateur)).FirstOrDefault()!;
	}
	public UtilisateurDto GetUserFilterByEmailAddress(string email)
	{
		return context.GetUsersData().Where(u => u.Email.ToUpper().Equals(email.ToUpper())).FirstOrDefault()!;
	}
	public  UtilisateurDto GetUserByFilter(string email, bool? adminOnly = null, Func<IQueryable<UtilisateurDto>, IQueryable<UtilisateurDto>> filter = null)
	{
		IQueryable<UtilisateurDto> query = (IQueryable<UtilisateurDto>)context.GetUsersData();
		if (!string.IsNullOrEmpty(email))
		{
			query = query.Where(u => u.Email.ToUpper() == email.ToUpper());
			if (adminOnly.HasValue && adminOnly.Value)
			{
				query = query.Where(u => u.Role == UtilisateurDto.Privilege.Administrateur);
			}		
		}
		if (filter != null)
		{
			query = filter(query);
		}
		return  query.FirstOrDefault()!;
	}

}