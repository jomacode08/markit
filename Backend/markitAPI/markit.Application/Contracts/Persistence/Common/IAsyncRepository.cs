using markit.Domain.Common;
using System.Linq.Expressions;

namespace markit.Application.Contracts.Persistence.Common
{
    public interface IAsyncRepository<T> where T : BaseModel
    {
        #region Querys
        Task<IReadOnlyList<T>> GetAllAsync();
        Task<T?> GetByIdAsync(int id, string? includeString = null);
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>> expression);

        // Ordenamiento
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
                                        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                        string? includeString = null,
                                        bool disableTracking = true);
        // Paginación
        Task<IReadOnlyList<T>> GetAsync(Expression<Func<T, bool>>? expression = null,
                                        Func<IQueryable<T>, IOrderedQueryable<T>>? orderBy = null,
                                        List<Expression<Func<T, object>>>? includes = null,
                                        bool disableTracking = true);
        #endregion

        #region Commands
        Task<T> AddAsync(T entity);
        Task<T> UpdateAsync(T entity);
        Task HardDeleteAsync(T entity);
        Task SoftDeleteAsync(T entity);

        void AddEntity(T entity);
        void AddRangeEntity(List<T> entityList);
        void UpdateEntity(T entity);
        void HardDeleteEntity(T entity);
        void SoftDeleteEntity(T entity);
        void SoftDeleteRangeEntity(List<T> entityList);
        #endregion
    }
}
