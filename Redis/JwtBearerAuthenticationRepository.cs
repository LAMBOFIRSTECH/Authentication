// using Authentifications.Middlewares;
// using Authentifications.Models;
// using Authentifications.Services;

// namespace Authentifications.Repositories;
// public class JwtBearerAuthenticationRepository
// {
// 	private readonly RedisCacheService redisCache;
// 	public JwtBearerAuthenticationRepository(RedisCacheService redisCache)
// 	{
// 		this.redisCache = redisCache;
// 	}
// 	// public UtilisateurDto GetUserByFilter(string email, bool? adminOnly = null) //Revoir pas besoin de rechercher la liste des utilisateurs seul le user avec le bon mail suffit
// 	// {
// 	// 	IQueryable<UtilisateurDto> query = context.GetUsersDataAsync().Result.AsQueryable();
// 	// 	if (!string.IsNullOrEmpty(email))
// 	// 	{
// 	// 		query = query.Where(u => u.Email!.ToUpper() == email.ToUpper());
// 	// 		if (adminOnly.HasValue && adminOnly.Value && query.Any(u => u.Role == UtilisateurDto.Privilege.Administrateur))
// 	// 		{
// 	// 			query = query.Where(u => u.Role == UtilisateurDto.Privilege.Administrateur);
// 	// 		}
// 	// 		else if (adminOnly.HasValue && adminOnly.Value && query.Any(u => u.Role == UtilisateurDto.Privilege.Utilisateur))
// 	// 		{
// 	// 			throw new AuthentificationBasicException("This user doesn't have the right privilege for JWT Token.");
// 	// 		}
// 	// 	}
// 	// 	return query.FirstOrDefault()!;
// 	// }
// 	public async Task<UtilisateurDto> GetUserByEmail(string email, bool? adminOnly)
// 	{
// 		var utilisateur = await redisCache.GetCredentialsAsync(email,null);
// 		if (utilisateur == null)
// 		{
// 			throw new AuthentificationBasicException("No data to retrieve user doesn't exist");
// 		}
// 		if (adminOnly.HasValue && adminOnly.Value && utilisateur.Role == UtilisateurDto.Privilege.Utilisateur)
// 		{
// 			throw new AuthentificationBasicException("This user doesn't have the right privilege for JWT Token.");
// 		}
// 		return utilisateur;
// 	}
// 	public async Task<UtilisateurDto> GetUserByEmails(string email, string password)
// 	{
// 		return await redisCache.GetCredentialsAsync(email,password);
// 		// if (utilisateur == null)
// 		// {
// 		// 	throw new AuthentificationBasicException("No data to retrieve user doesn't exist");
// 		// }
// 	}
// }



	// string clientID = redisCacheService.GenerateClientId(email, password);
			// // tester la connexion à la bd avant de save
			// var isConnected = redisDatabase.Multiplexer.IsConnected;
			// if (!isConnected)
			// {
			// 	throw new InvalidOperationException("Redis n'est pas connecté.");
			// }
			// log.LogInformation("Redis est connecté : {IsConnected}", isConnected);

			// await redisCacheService.StoreCredentialsAsync(clientID, email, password);
			// if (await AuthenticateAsync(email, password) == false)
			// {
			// 	return AuthenticateResult.Fail("Invalid email or password");
			// }