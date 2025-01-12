using Authentifications.DataBaseContext;
using Authentifications.Middlewares;
using Authentifications.Models;

namespace Authentifications.Repositories;
public class JwtBearerAuthenticationRepository
{
	private readonly ApiContext context;
	public JwtBearerAuthenticationRepository(ApiContext context)
	{
		this.context = context;
	}
	public UtilisateurDto GetUserByFilter(string email, bool? adminOnly = null) //Revoir pas besoin de rechercher la liste des utilisateurs seul le user avec le bon mail suffit
	{
		IQueryable<UtilisateurDto> query = context.GetUsersDataAsync().Result.AsQueryable();
		if (!string.IsNullOrEmpty(email))
		{
			query = query.Where(u => u.Email!.ToUpper() == email.ToUpper());
			if (adminOnly.HasValue && adminOnly.Value && query.Any(u => u.Role == UtilisateurDto.Privilege.Administrateur))
			{
				query = query.Where(u => u.Role == UtilisateurDto.Privilege.Administrateur);
			}
			else if (adminOnly.HasValue && adminOnly.Value && query.Any(u => u.Role == UtilisateurDto.Privilege.Utilisateur))
			{
				throw new AuthentificationBasicException("This user doesn't have the right privilege for JWT Token.");
			}
		}
		return query.FirstOrDefault()!;
	}
}