using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Authentifications.Models
{
    public class Login
    {
        [Required]
        public string Nom { get; set; } = string.Empty;
        [Required]
        public string? Email { get; set; }
        public enum Privilege { Administrateur, Utilisateur }
        [EnumDataType(typeof(Privilege))]
        [Required]
        public Privilege Role { get; set; }
    }
}