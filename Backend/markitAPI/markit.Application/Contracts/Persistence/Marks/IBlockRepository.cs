using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;

namespace markit.Application.Contracts.Persistence.Marks
{
    public interface IBlockRepository : IAsyncRepository<Block>
    {
        Task<IEnumerable<BlockSearchResult>> SearchAsync(string searchTerm, int creatorId, CancellationToken cancellationToken);
    }
}
