using markit.Application.Models.Authentication.Enums;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace markit.Application.Models.Authentication.AppUser
{
    public class AppUser : IdentityUser
    {
        public int? CreatorId { get; set; }
        public string GivenName { get; set; } = string.Empty;
        public string? Picture { get; set; }

        [Required]
        public AccessType AccessType { get; set; }

        [Required]
        public bool RegistrationConfirmed { get; set; } = false;

        [Required]
        public DateTime CreatedDate { get; set; }
    }
}
