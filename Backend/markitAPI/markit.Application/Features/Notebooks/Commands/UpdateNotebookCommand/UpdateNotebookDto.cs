using markit.Application.Features.Blocks.Queries.ViewModels;

namespace markit.Application.Features.Notebooks.Commands.UpdateNotebookCommand
{
    public class UpdateNotebookDto
    {
        public string InputName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public List<BlockViewModel> Blocks { get; set; } = new();
    }
}
