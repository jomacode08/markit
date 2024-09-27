using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace markit.Infraestructure.Security.Models
{
    public class User : IdentityUser
    {
        [Required, MaxLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string LastName { get; set; } = string.Empty;
        
        public Gender? Gender { get; set; }

        public DateOnly? BirthDate { get; set; }
        
        [Required]
        public DateTime CreatedDate { get; set; }

        [NotMapped]
        public string FullName
        {
            get
            {
                return $"{FirstName} {LastName}";
            }
        }
    }

    public enum Gender
    {
        Female = 1,
        Masculine = 2,
        Other = 3
    }
}
