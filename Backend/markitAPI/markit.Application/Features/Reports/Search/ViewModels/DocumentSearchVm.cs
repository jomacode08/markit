using markit.Application.Models.MeiliSearch.Documents;

namespace markit.Application.Features.Reports.Search.ViewModels
{
    public class DocumentSearchVm
    {
        public IReadOnlyList<CollectionDocument> Collections { get; set; } = [];
        public IReadOnlyList<MarkDocument> Marks { get; set; } = [];
    }
}
