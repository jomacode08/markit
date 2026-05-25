using markit.Application.Features.Blocks.Queries.ViewModels;

namespace markit.Application.Features.Marks.Commands.UpdateMarkCommand
{
    public class UpdateMarkDto
    {
        public string InputName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public List<BlockViewModel> Blocks { get; set; } = new();
    }
}
