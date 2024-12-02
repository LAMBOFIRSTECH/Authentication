using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Authentifications.Models
{
    public class UtilisateurDto
    {
        /// <summary>
        /// Représente l'identifiant unique d'un utilisateur.
        /// </summary>
        [Key]
        //[System.Text.Json.Serialization.JsonIgnore]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
        [Required]
        public string Nom { get; set; } = string.Empty;

        [Required(ErrorMessage = "L'email est requis.")]
        [EmailAddress(ErrorMessage = "L'adresse email est invalide.")]
        public string Email { get; set; } = string.Empty;

        public enum Privilege { Administrateur, Utilisateur }
        [EnumDataType(typeof(Privilege))]
        [Required]
        public Privilege Role { get; set; }

        [Required]
        [Category("Security")]
        public string Pass { get; set; } = string.Empty;

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
}
