using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Domain.Entities;

namespace markit.Application.Contracts.Persistence.Notebooks
{
    public interface IBlockRepository : IAsyncRepository<Block>
    {
        Task<IEnumerable<BlockSearchResult>> SearchAsync(string searchTerm, string userId, CancellationToken cancellationToken);
    }
}
