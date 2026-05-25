using markit.Application.Features.Blocks.Queries.ViewModels;

namespace markit.Application.Features.Marks.Queries.ViewModels
{
    public class MarkViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string InputName { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CollectionId { get; set; }
        public string CollectionName {  get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public DateTime? CreatedDate { get; set; }

        public List<BlockViewModel> Blocks { get; set; } = new();
    }
}
