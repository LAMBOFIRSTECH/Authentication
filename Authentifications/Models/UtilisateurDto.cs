using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Authentifications.Models;
public class UtilisateurDto
{

	/// <summary>
	/// Représente l'identifiant unique d'un utilisateur.
	/// </summary>
	[Key]
	[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public Guid ID { get; set; }
	[MaxLength(20, ErrorMessage = "Username cannot exceed 20 characters")]
	public string Nom { get; set; } = string.Empty; // A terme on aura le nom complet de l'utilisateur {Nom + Prenom}
	[Required]
	[EmailAddress]
	public string? Email { get; set; }
	public enum Privilege { Administrateur, Utilisateur }
	[EnumDataType(typeof(Privilege))]
	[Required]
	public Privilege Role { get; set; }
	[Required]
	[Category("Security")]
	public string? Pass { get; set; }
	public bool CheckHashPassword(string password)
	{
		return BCrypt.Net.BCrypt.Verify(password, Pass);
	}
	public bool CheckEmailAdress(string email)
	{
		string regexMatch = "(?<alpha>\\w+)@(?<mailing>[aA-zZ]+)\\.(?<domaine>[aA-zZ]+$)";
		if (string.IsNullOrEmpty(email))
		{
			return false;
		}
		Match check = Regex.Match(email, regexMatch);
		return check.Success;
	}
}

public class LoginRequest : UtilisateurDto
{
	[Required]
	public bool State { get; set; }
	public LoginRequest(){}
}