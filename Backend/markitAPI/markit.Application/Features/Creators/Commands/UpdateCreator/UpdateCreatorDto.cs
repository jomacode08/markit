using markit.Domain.Entities;

namespace markit.Application.Features.Creators.Commands.UpdateCreator
{
    public class UpdateCreatorDto
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public Gender Gender { get; set; }
    }
}
