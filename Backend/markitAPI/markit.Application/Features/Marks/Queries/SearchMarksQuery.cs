using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Marks.Queries.ViewModels;
using MediatR;

namespace markit.Application.Features.Marks.Queries
{
    public class SearchMarksQuery : IRequest<IReadOnlyList<MarkSearchResult>>
    {
        public string SearchTerm { get; }
        public string UserId { get; }

        public SearchMarksQuery(string searchTerm, string userId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));
            
            SearchTerm = searchTerm;
            UserId = userId;
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
                userId: request.UserId,
                cancellationToken
            );

            IEnumerable<MarkSearchResult> marksByName = await SearchMarksByNameAsync(
                searchTerm: request.SearchTerm,
                userId: request.UserId,
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
            string userId,
            CancellationToken cancellationToken
        ) 
        {
            IEnumerable<BlockSearchResult> blocks = await _unitOfWork.BlockRepository.SearchAsync(searchTerm, userId, cancellationToken);
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
            string userId,
            CancellationToken cancellationToken
        )
        {
            return await _unitOfWork.MarkRepository.SearchAsync(searchTerm, userId, cancellationToken);
        }
    }
}
