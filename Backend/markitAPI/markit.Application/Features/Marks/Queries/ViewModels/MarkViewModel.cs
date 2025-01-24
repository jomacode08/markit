namespace markit.Application.Features.Marks.Queries.ViewModels
{
    public class MarkViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CreatorId { get; set; }
    }
}
