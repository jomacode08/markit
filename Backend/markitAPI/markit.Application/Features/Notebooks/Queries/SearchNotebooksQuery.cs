using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using MediatR;

namespace markit.Application.Features.Notebooks.Queries
{
    public class SearchNotebooksQuery : IRequest<IReadOnlyList<NotebookSearchResult>>
    {
        public string SearchTerm { get; }
        public string UserId { get; }

        public SearchNotebooksQuery(string searchTerm, string userId)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                throw new ArgumentException("Search term cannot be empty", nameof(searchTerm));
            
            SearchTerm = searchTerm;
            UserId = userId;
        }
    }

    public class SearchNotebooksQueryHandler : IRequestHandler<SearchNotebooksQuery, IReadOnlyList<NotebookSearchResult>>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SearchNotebooksQueryHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<IReadOnlyList<NotebookSearchResult>> Handle(SearchNotebooksQuery request, CancellationToken cancellationToken)
        {
            List<NotebookSearchResult> results = await SearchNotebooksByContentAsync(
                searchTerm: request.SearchTerm,
                userId: request.UserId,
                cancellationToken
            );

            IEnumerable<NotebookSearchResult> notebooksByName = await SearchNotebooksByNameAsync(
                searchTerm: request.SearchTerm,
                userId: request.UserId,
                cancellationToken
            );

            Dictionary<int, NotebookSearchResult> notebookDictionary = results.ToDictionary(m => m.Id);
            foreach (NotebookSearchResult notebook in notebooksByName) 
            {
                if (!notebookDictionary.ContainsKey(notebook.Id))
                {
                    results.Add(notebook);
                }
            }

            return results;
        }

        private async Task<List<NotebookSearchResult>> SearchNotebooksByContentAsync(
            string searchTerm,
            string userId,
            CancellationToken cancellationToken
        ) 
        {
            IEnumerable<BlockSearchResult> blocks = await _unitOfWork.BlockRepository.SearchAsync(searchTerm, userId, cancellationToken);
            var blockDictionary = blocks.GroupBy(b => b.NotebookId).ToDictionary(g => g.Key);
            List<NotebookSearchResult> notebooks = [];

            foreach (int notebookId in blockDictionary.Keys)
            {
                BlockSearchResult firstCoincidence = blockDictionary[notebookId].First();
                NotebookSearchResult notebook = new()
                {
                    Id = notebookId,
                    Name = firstCoincidence.NotebookName,
                    Blocks = blockDictionary[notebookId]
                };
                notebooks.Add(notebook);
            }

            return notebooks;
        }

        private async Task<IEnumerable<NotebookSearchResult>> SearchNotebooksByNameAsync(
            string searchTerm,
            string userId,
            CancellationToken cancellationToken
        )
        {
            return await _unitOfWork.NotebookRepository.SearchAsync(searchTerm, userId, cancellationToken);
        }
    }
}
