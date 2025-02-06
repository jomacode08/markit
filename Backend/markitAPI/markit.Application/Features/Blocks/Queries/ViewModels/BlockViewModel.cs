using markit.Domain.Entities;

namespace markit.Application.Features.Blocks.Queries.ViewModels
{
    public class BlockViewModel
    {
        public int Id { get; set; }
        public int Cols { get; set; }
        public string? Title { get; set; }
        public BackColors Color { get; set; }
        public string? Content { get; set; }
    }
}
