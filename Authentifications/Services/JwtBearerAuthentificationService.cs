using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Authentifications.Interfaces;
using Authentifications.Middlewares;
using Authentifications.Models;
using Authentifications.Repositories;
using Microsoft.IdentityModel.Tokens;
namespace Authentifications.Services;
public class JwtBearerAuthentificationService : IJwtToken
{
	private readonly JwtBearerAuthentificationRepository jwtBearerAuthentificationRepository;
	private readonly IConfiguration configuration;
	public JwtBearerAuthentificationService(IConfiguration configuration, JwtBearerAuthentificationRepository jwtBearerAuthentificationRepository)

	{
		this.jwtBearerAuthentificationRepository = jwtBearerAuthentificationRepository;
		this.configuration = configuration;

	}
	public bool CheckUserSecret(string secretPass)
	{
		string secretUserPass = configuration["ConnectionStrings:SecretApiKey"]; //La clé privée est ici la clé publique dans TasksManagement_API Dev configuration["JwtSettings:JwtSecretKey"]; // Prod Environment.GetEnvironmentVariable("PasswordSecret")!; //

		if (string.IsNullOrEmpty(secretUserPass))
		{
			throw new ArgumentException("La clé secrete est inexistante");
		}
		var Pass = BCrypt.Net.BCrypt.HashPassword(secretPass);
		var BCryptResult = BCrypt.Net.BCrypt.Verify(secretUserPass, Pass);
		if (!BCryptResult.Equals(true)) { return false; }
		return true;
	}
	public async Task<TokenResult> GetToken(string email, string password)
	{
		var utilisateur = jwtBearerAuthentificationRepository.GetUserFilterByEmailAddress(email);
		if (utilisateur.Role == UtilisateurDto.Privilege.Utilisateur)
		{
			throw new AuthentificationBasicException($"{utilisateur.Nom} doesn't have the right privilege for JWT Token.");
		}

		await Task.Delay(500);
		return new TokenResult
		{
			Response = true,
			Token = GenerateJwtToken(utilisateur.Email)
		};

	}
	public string GetSigningKey()
	{
		var JwtSettings = configuration.GetSection("JwtSettings");
		int secretKeyLength = int.Parse(JwtSettings["JwtSecretKey"]); // en faite c'est ça la clé d'api que l'on génère depuis hcp (hashicorp)
		var randomSecretKey = new RandomUserSecret(); // n'a pas lieu d'exister
		string signingKey = randomSecretKey.GenerateRandomKey(secretKeyLength);
		return signingKey;
	}
	public string GenerateJwtToken(string email)
	{

		var utilisateur = jwtBearerAuthentificationRepository.GetUserWithAdminPrivilege(email);
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey()));
		var tokenHandler = new JwtSecurityTokenHandler();
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[] {
					new Claim(ClaimTypes.Name, utilisateur.Nom),
					new Claim(ClaimTypes.Email, utilisateur.Email),
					new Claim(ClaimTypes.Role, utilisateur.Role.ToString())
				}
			),
			Expires = DateTime.UtcNow.AddMinutes(1440),
			SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature),
			Audience = configuration.GetSection("JwtSettings")["Audience"],
			Issuer = configuration.GetSection("JwtSettings")["Issuer"],
		};
		var tokenCreation = tokenHandler.CreateToken(tokenDescriptor);
		var token = tokenHandler.WriteToken(tokenCreation);
		return token;

	}
}