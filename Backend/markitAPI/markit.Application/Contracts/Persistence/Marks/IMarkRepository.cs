using markit.Application.Contracts.Persistence.Common;
using markit.Domain.Entities;

namespace markit.Application.Contracts.Persistence.Marks
{
    public interface IMarkRepository : IAsyncRepository<Mark>
    {
        Task<Mark?> GetWithOrderedBlocks(int id);
    }
}
