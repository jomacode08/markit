using markit.Application.Contracts.MeiliSearch;
using markit.Application.Features.Reports.Search.ViewModels;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Application.Models.MeiliSearch.Search;
using MediatR;

namespace markit.Application.Features.Reports.Search
{
    public class DocumentSearchQuery : IRequest<DocumentSearchVm>
    {
        public string Query { get; set; } = string.Empty;
        public DocumentSearchFilter Filter { get; set; }
        public int CreatorId { get; set; }
    }

    public class DocumentSearchQueryHandler : IRequestHandler<DocumentSearchQuery, DocumentSearchVm>
    {
        private readonly IDocumentRepository<CollectionDocument> _collectionDocumentRepository;
        private readonly IDocumentRepository<MarkDocument> _markDocumentRepository;

        public DocumentSearchQueryHandler(
            IDocumentRepository<CollectionDocument> collectionDocumentRepository,
            IDocumentRepository<MarkDocument> markDocumentRepository
        )
        {
            _collectionDocumentRepository = collectionDocumentRepository;
            _markDocumentRepository = markDocumentRepository;
        }

        public async Task<DocumentSearchVm> Handle(DocumentSearchQuery request, CancellationToken cancellationToken)
        {
            bool requireAll = request.Filter.Equals(DocumentSearchFilter.All);

            IReadOnlyList<CollectionDocument> collections = requireAll || request.Filter.Equals(DocumentSearchFilter.Collections)
                ? await _collectionDocumentRepository.FormattedSearchAsync(new FormattedDocumentSearch(
                    Query: request.Query,
                    AttributesToHighlight: ["name"],
                    Limit: 10,
                    CreatorId: request.CreatorId)
                ): [];

            IReadOnlyList<MarkDocument> marks = requireAll || request.Filter.Equals(DocumentSearchFilter.Marks)
                ? await _markDocumentRepository.FormattedSearchAsync(new FormattedDocumentSearch(
                    Query: request.Query,
                    AttributesToHighlight: ["name"],
                    Limit: 10,
                    CreatorId: request.CreatorId)
                ) : [];

            return new DocumentSearchVm
            {
                Collections = collections,
                Marks = marks
            };
        }
    }
}
