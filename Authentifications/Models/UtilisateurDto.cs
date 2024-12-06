using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.RegularExpressions;

namespace Authentifications.Models
{
    public record UtilisateurDto
    {
        /// <summary>
        /// Représente l'identifiant unique d'un utilisateur.
        /// </summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public Guid ID { get; set; }
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
    }
}
