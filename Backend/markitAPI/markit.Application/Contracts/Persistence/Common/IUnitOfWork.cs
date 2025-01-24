using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Contracts.Persistence.Users;
using markit.Domain.Common;

namespace markit.Application.Contracts.Persistence.Common
{
    public interface IUnitOfWork : IDisposable
    {
        ICreatorRepository creatorRepository { get; }
        IMarkRepository markRepository { get; }

        IAsyncRepository<TEntity> Repository<TEntity>() where TEntity : BaseModel;
        Task<int> Complete();
    }
}
