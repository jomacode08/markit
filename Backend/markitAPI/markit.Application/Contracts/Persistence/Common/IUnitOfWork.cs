using markit.Domain.Common;

namespace markit.Application.Contracts.Persistence.Common
{
    public interface IUnitOfWork : IDisposable
    {
        IAsyncRepository<TEntity> Repository<TEntity>() where TEntity : BaseModel;
        Task<int> Complete();
    }
}
