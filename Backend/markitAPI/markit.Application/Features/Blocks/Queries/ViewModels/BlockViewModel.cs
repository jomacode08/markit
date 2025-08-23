using markit.Domain.Entities;

namespace markit.Application.Features.Blocks.Queries.ViewModels
{
    public class BlockViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Content { get; set; }
        public int Order {  get; set; }
        public DateTime? CreatedDate { get; set; }
    }
}
