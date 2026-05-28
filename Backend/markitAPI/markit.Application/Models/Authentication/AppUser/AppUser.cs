using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace markit.Application.Models.Authentication.AppUser
{
    public class AppUser : IdentityUser
    {
        [Required] public string GivenName { get; set; } = string.Empty;
        [Required] public AccessType AccessType { get; set; }
        [Required] public bool Enabled { get; set; } = false;
        [Required] public DateTime CreatedDate { get; set; }
        public string? Picture { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public virtual ICollection<Collection>? Collections {  get; set; }
    }
}
