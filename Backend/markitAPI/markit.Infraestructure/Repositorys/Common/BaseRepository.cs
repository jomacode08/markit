using markit.Application.Contracts.Persistence.Common;
using markit.Domain.Common;
using markit.Infraestructure.Persistence.EF;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace markit.Infraestructure.Repositorys
{
    public class BaseRepository<T> : IAsyncRepository<T> where T : BaseModel
    {
        protected readonly MarkitDbContext context;

        public BaseRepository(MarkitDbContext mirefDbContext)
        {
            context = mirefDbContext;
        }

        #region Queries
        public async Task<IReadOnlyList<T>> GetAllAsync()
        {
            return await context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(int id, string? includeString = null)
        {
            IQueryable<T> query = context.Set<T>();

            if (!string.IsNullOrWhiteSpace(includeString))
            {
                var includes = includeString.Split(',');

                foreach (var include in includes)
                {
                    query = query.Include(include);
                }
            }

            return await query.FirstOrDefaultAsync(e => e.Id.Equals(id));
        }

        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> expression)
        {
            return await context.Set<T>().Where(expression).ToListAsync();
        }

        // Ordenamiento y Joins
        public async Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
                                                     Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                                     string? includeString = null,
                                                     bool disableTracking = true)
        {
            // Instanciar IQueryable
            IQueryable<T> query = context.Set<T>();

            // Evaluar si el parámetro bandera disableTrackind esta activo, para desactivar el tracking en los datos retornados.
            if (disableTracking) 
                query = query.AsNoTracking();

            // Evaluar params de la consulta
            if (!string.IsNullOrWhiteSpace(includeString)) 
                query = query.Include(includeString);

            if (expression != null)
                query = query.Where(expression);

            // Evaluar y aplicar OrderBy
            if (orderBy != null)
                return await orderBy(query).ToListAsync();

            return await query.ToListAsync();
        }

        public async Task<IReadOnlyList<T>> GetAsyncOffsetPagination(int offset,
                                               int limit,
                                               Expression<Func<T, bool>>? expression = null,
                                               Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                               List<Expression<Func<T, object>>>? includes = null,
                                               bool disableTracking = true)
        {
            /** Applying pagination **/
            IQueryable<T> query = context.Set<T>()
                .Where(expression ?? (e => e.Id > 0))
                .Skip((offset - 1) * limit)
                .Take(limit);

            /** Handling optional params **/
            if (disableTracking) query = query.AsNoTracking();
            if (includes != null)
                query = includes.Aggregate(query, 
                        (current, include) => current.Include(include));
            if (orderBy != null) return await orderBy(query).ToListAsync();

            return await query.ToListAsync();
        }
        #endregion

        #region Commands
        // Commands operations with save changes in context
        public async Task<T> AddAsync(T entity)
        {
            context.Set<T>().Add(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<T> UpdateAsync(T entity)
        {
            context.Set<T>().Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task HardDeleteAsync(T entity)
        {
            context.Set<T>().Remove(entity);
            await context.SaveChangesAsync();
        }
        public async Task SoftDeleteAsync(T entity)
        {
            entity.Enable = false;
            await UpdateAsync(entity);
        }

        // Commands operations in memory
        public void AddEntity(T entity)
        {
            context.Set<T>().Add(entity);
        }

        public void AddRangeEntity(List<T> entityList)
        {
            context.Set<T>().AddRange(entityList);
        }

        public void UpdateEntity(T entity)
        {
            context.Set<T>().Attach(entity);
            context.Entry(entity).State = EntityState.Modified;
        }

        public void HardDeleteEntity(T entity)
        {
            context.Set<T>().Remove(entity);
        }

        public void SoftDeleteEntity(T entity)
        {
            entity.Enable = false;
            UpdateEntity(entity);
        }

        public void SoftDeleteRangeEntity(List<T> entityList)
        {
            foreach (var entity in entityList)
            {
                SoftDeleteEntity(entity);
            }
        }
        #endregion
    }
}
