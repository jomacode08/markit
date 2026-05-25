namespace markit.Application.Features.Marks.Commands.RenameMarkCommand
{
    public class RenameMarkDto
    {
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Emoji { get; set; }
    }
}
