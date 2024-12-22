using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Authentifications.Models;
public class LoginRequest
{
	public string? Email { get; set; }
	public enum Privilege { Administrateur}
	[EnumDataType(typeof(Privilege))]
	[Required]
	public Privilege Role { get; set; }
	[Required]
	[Category("Security")]
	public string? Pass { get; set; }
	public string SetHashPassword(string password)
	{
		if (!string.IsNullOrEmpty(password))
		{
			Pass = BCrypt.Net.BCrypt.HashPassword($"{password}");
		}
		return Pass!;
	}
}