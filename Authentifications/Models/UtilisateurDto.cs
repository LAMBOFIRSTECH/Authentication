using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Authentifications.Models;
public record UtilisateurDto
{
	/// <summary>
	/// Représente l'identifiant unique d'un utilisateur.
	/// </summary>
	[Key]
	// [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
	public string? clientID { get; set; } 
	[Required]
	public string? Nom { get; set; }

	[Required(ErrorMessage = "L'email est requis.")]
	[EmailAddress(ErrorMessage = "L'adresse email est invalide.")]
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
	public string SetHashPassword(string password)
	{
		if (!string.IsNullOrEmpty(password))
		{
			Pass = BCrypt.Net.BCrypt.HashPassword($"{password}");
		}
		return Pass!;
	}
}
