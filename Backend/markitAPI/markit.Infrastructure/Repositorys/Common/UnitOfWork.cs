using markit.Application.Contracts.Persistence.Common;
using markit.Application.Contracts.Persistence.Marks;
using markit.Domain.Entities;
using markit.infrastructure.Persistence.EF;
using markit.infrastructure.Repositorys.Marks;

namespace markit.infrastructure.Repositorys.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MarkitDbContext _context;
        #region Generic repositories
        public  IAsyncRepository<Creator> CreatorRepository { get; private set; }
        public IAsyncRepository<Block> BlockRepository { get; private set; }
        #endregion
        #region Custom repositories
        public ICollectionRepository CollectionRepository { get; private set; }
        public IMarkRepository MarkRepository { get; private set; }
        #endregion

        public UnitOfWork(MarkitDbContext context)
        {
            _context = context;
            CreatorRepository = new BaseRepository<Creator>(context);
            BlockRepository = new BaseRepository<Block>(context);
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
