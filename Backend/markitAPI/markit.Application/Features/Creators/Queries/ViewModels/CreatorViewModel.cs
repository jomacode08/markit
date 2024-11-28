using markit.Domain.Entities;

namespace markit.Application.Features.Creators.Queries.ViewModels
{
    public class CreatorViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public Gender? Gender { get; set; }
        public string? BirthDate { get; set; }

        // System Access properties
        public string Email { get; set; } = string.Empty;
        public string? Picture { get; set; }
        public bool RegistrationConfirmed { get; set; }
    }
}
