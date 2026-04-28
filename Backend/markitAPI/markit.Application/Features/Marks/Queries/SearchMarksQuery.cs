using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Marks.Queries.ViewModels;
using MediatR;

namespace markit.Application.Features.Marks.Queries
{
    public class SearchMarksQuery : IRequest<IReadOnlyList<MarkSearchResult>>
    {
        public string SearchTerm { get; }
        public int CreatorId { get; }

        public SearchMarksQuery(string searchTerm, int creatorId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));
            
            SearchTerm = searchTerm;
            CreatorId = creatorId;
        }
    }

    public class SearchMarksQueryHandler : IRequestHandler<SearchMarksQuery, IReadOnlyList<MarkSearchResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchMarksQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<MarkSearchResult>> Handle(SearchMarksQuery request, CancellationToken cancellationToken)
        {
            List<MarkSearchResult> results = await SearchMarksByContentAsync(
                searchTerm: request.SearchTerm,
                creatorId: request.CreatorId,
                cancellationToken
            );

            IEnumerable<MarkSearchResult> marksByName = await SearchMarksByNameAsync(
                searchTerm: request.SearchTerm,
                creatorId: request.CreatorId,
                cancellationToken
            );

            Dictionary<int, MarkSearchResult> markDictionary = results.ToDictionary(m => m.Id);
            foreach (MarkSearchResult mark in marksByName) 
            {
                if (!markDictionary.ContainsKey(mark.Id))
                {
                    results.Add(mark);
                }
            }

            return results;
        }

        private async Task<List<MarkSearchResult>> SearchMarksByContentAsync(
            string searchTerm,
            int creatorId,
            CancellationToken cancellationToken
        ) 
        {
            IEnumerable<BlockSearchResult> blocks = await _unitOfWork.BlockRepository.SearchAsync(searchTerm, creatorId, cancellationToken);
            var blockDictionary = blocks.GroupBy(b => b.MarkId).ToDictionary(g => g.Key);
            List<MarkSearchResult> marks = [];

            foreach (int markId in blockDictionary.Keys)
            {
                BlockSearchResult firstCoincidence = blockDictionary[markId].First();
                MarkSearchResult mark = new()
                {
                    Id = markId,
                    Name = firstCoincidence.MarkName,
                    Blocks = blockDictionary[markId]
                };
                marks.Add(mark);
            }

            return marks;
        }

        private async Task<IEnumerable<MarkSearchResult>> SearchMarksByNameAsync(
            string searchTerm,
            int creatorId,
            CancellationToken cancellationToken
        )
        {
            return await _unitOfWork.MarkRepository.SearchAsync(searchTerm, creatorId, cancellationToken);
        }
    }
}
