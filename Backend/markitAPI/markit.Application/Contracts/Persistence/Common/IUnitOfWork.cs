
using markit.Application.Contracts.Persistence.Notebooks;

namespace markit.Application.Contracts.Persistence.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IBlockRepository BlockRepository { get; }
        ICollectionRepository CollectionRepository { get; }
        INoteBookRepository NotebookRepository { get; }
        Task<int> Complete();
    }
}
