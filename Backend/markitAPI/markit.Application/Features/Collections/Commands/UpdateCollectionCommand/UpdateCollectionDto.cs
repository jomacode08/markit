namespace markit.Application.Features.Collections.Commands.UpdateCollectionCommand
{
    public class UpdateCollectionDto
    {
        public string Name { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public int CreatorId { get; set; }
    }
}
