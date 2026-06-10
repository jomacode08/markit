namespace markit.Application.Features.Notebooks.Commands.RenameNotebookCommand
{
    public class RenameNotebookDto
    {
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Emoji { get; set; }
    }
}
