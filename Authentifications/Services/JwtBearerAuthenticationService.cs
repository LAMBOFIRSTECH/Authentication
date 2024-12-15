using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Authentifications.Interfaces;
using Authentifications.Models;
using Authentifications.Repositories;
using Microsoft.IdentityModel.Tokens;
namespace Authentifications.Services;
public class JwtBearerAuthenticationService : IJwtToken
{
	private readonly JwtBearerAuthenticationRepository jwtBearerAuthenticationRepository;
	private readonly IConfiguration configuration;
	public JwtBearerAuthenticationService(IConfiguration configuration, JwtBearerAuthenticationRepository jwtBearerAuthenticationRepository)
	{
		this.jwtBearerAuthenticationRepository = jwtBearerAuthenticationRepository;
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
	public async Task<TokenResult> GetToken(string email,string password)
	{
		var utilisateur = jwtBearerAuthenticationRepository.GetUserByEmails(email, password);
		if(utilisateur is null)
		{
			throw new KeyNotFoundException();
		}
		await Task.Delay(500);
		return new TokenResult
		{
			Response = true,
			Token = GenerateJwtToken(utilisateur.Result.Email!)
		};
	}
	public string GetSigningKey()
	{
		RSA rsa = RSA.Create(2048);
		RSAParameters privateKey = rsa.ExportParameters(true);
		RSAParameters publicKey = rsa.ExportParameters(false); // Rotation des clés

		var rsaSecurityKey = new RsaSecurityKey(rsa);
		return rsaSecurityKey.ToString();
	}
	public string GenerateJwtToken(string email)
	{
		string password= string.Empty;
		var utilisateur = jwtBearerAuthenticationRepository.GetUserByEmails(email,password);
		var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(GetSigningKey()));
		var tokenHandler = new JwtSecurityTokenHandler();
		var tokenDescriptor = new SecurityTokenDescriptor
		{
			Subject = new ClaimsIdentity(new[] {	
					new Claim(ClaimTypes.Email, utilisateur.Result.Email!),
					//new Claim(ClaimTypes.Role, utilisateur.Result.Role.ToString()),
					new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
					new Claim(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
				}
			),
			Expires = DateTime.UtcNow.AddHours(1),
			SigningCredentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha512Signature),
			Audience = null,
			Issuer = configuration.GetSection("JwtSettings")["Issuer"],
		};
		var additionalAudiences = new[] { "https://audience2.com", "https://localhost:9500" };
		tokenDescriptor.Claims = new Dictionary<string, object>
		{
			{ JwtRegisteredClaimNames.Aud, additionalAudiences }
		};
		var tokenCreation = tokenHandler.CreateToken(tokenDescriptor);
		var token = tokenHandler.WriteToken(tokenCreation);
		return token;
	}
}