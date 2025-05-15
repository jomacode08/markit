using markit.Application.Contracts.Persistence.Common;
using markit.Application.Contracts.Persistence.Marks;
using markit.Application.Contracts.Persistence.Users;
using markit.Domain.Common;
using markit.Infraestructure.Persistence.EF;
using markit.Infraestructure.Repositorys.Marks;
using markit.Infraestructure.Repositorys.Users;
using System.Collections;

namespace markit.Infraestructure.Repositorys.Common
{
    public class UnitOfWork : IUnitOfWork
    {
        private Hashtable? _repositories;
        private readonly MarkitDbContext _context;

        #region Inyección de Repositorios personalizados
        private ICollectionRepository _collectionRepository;
        private ICreatorRepository  _creatorRepository;
        private IMarkRepository     _markRepository;
        private IBlockRepository _blockRepository;

        public ICollectionRepository collectionRepository => _collectionRepository = new CollectionRepository(_context);
        public ICreatorRepository creatorRepository => _creatorRepository = new CreatorRepository(_context);
        public IMarkRepository markRepository => _markRepository = new MarkRepository(_context);
        public IBlockRepository blockRepository => _blockRepository = new BlockRepository(_context);
        #endregion

        public UnitOfWork(MarkitDbContext context)
        {
            _context = context;
        }

        public async Task<int> Complete()
        {
            return await _context.SaveChangesAsync();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        public IAsyncRepository<TEntity> Repository<TEntity>() where TEntity : BaseModel
        {
            // Inicializar repositories
            if (_repositories == null)
            {
                _repositories = new Hashtable();
            }

            // Obtener nombre de la entidad recibida
            string nameEntity = typeof(TEntity).Name;

            //Evaluar si el hashtable cuenta con la key de la entidad
            if (!_repositories.ContainsKey(nameEntity))
            {
                // Obtener el tipo del repositorio genérico
                Type repositoryType = typeof(BaseRepository<>);
                // Crear instancia del repositorio genérico
                var reposirotyInstance = Activator.CreateInstance(repositoryType.MakeGenericType(typeof(TEntity)), _context);

                // Agregarla al hashtable con la key de la entidad
                _repositories.Add(nameEntity, reposirotyInstance);
            }

            return (IAsyncRepository<TEntity>)_repositories[nameEntity]!;
        }
    }
}
