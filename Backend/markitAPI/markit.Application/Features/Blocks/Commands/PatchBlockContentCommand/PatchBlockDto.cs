namespace markit.Application.Features.Blocks.Commands.PatchBlockContentCommand
{
    public class PatchBlockDto
    {
        public string Content { get; set; } = string.Empty;
        public int CreatorId { get; set; }
    }
}
