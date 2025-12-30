using markit.Application.Contracts.Persistence.Marks;
using markit.Domain.Entities;

namespace markit.Application.Contracts.Persistence.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IAsyncRepository<Creator> CreatorRepository { get; }
        IAsyncRepository<Block> BlockRepository { get; }
        ICollectionRepository CollectionRepository { get; }
        IMarkRepository MarkRepository { get; }
        Task<int> Complete();
    }
}
