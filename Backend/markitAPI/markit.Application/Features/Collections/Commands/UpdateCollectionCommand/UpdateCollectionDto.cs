namespace markit.Application.Features.Collections.Commands.UpdateCollectionCommand
{
    public class UpdateCollectionDto
    {
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Emoji { get; set; }
    }
}
