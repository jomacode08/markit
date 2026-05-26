using markit.Application.Contracts.Persistence.Common;
using markit.Application.Contracts.Persistence.Marks;
using markit.Infrastructure.Persistence.EF;
using markit.Infrastructure.Repositories.Marks;

namespace markit.Infrastructure.Repositories.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MarkitDbContext _context;
        #region Custom repositories
        public IBlockRepository BlockRepository {  get; private set; }
        public ICollectionRepository CollectionRepository { get; private set; }
        public IMarkRepository MarkRepository { get; private set; }
        #endregion

        public UnitOfWork(MarkitDbContext context)
        {
            _context = context;
            BlockRepository = new BlockRepository(context);
            CollectionRepository = new CollectionRepository(context);
            MarkRepository = new MarkRepository(context);
        }

        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
