using System.Linq.Expressions;
using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Models.Filters;
using markit.Domain.Entities;

namespace markit.Application.Common.Helpers.Services
{
    public class CollectionItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        public async Task<List<CollectionItem>> GetCollectionItemsPaged(CollectionItemPagedFilter filter)
        {
            IReadOnlyList<Collection> collections = [];
            IReadOnlyList<Mark> marks = [];
            var items = new List<CollectionItem>();
            int numberOfItemsToDelivery = filter.PageSize;

            // Get children collections
            if (filter.CollectionItemCategory.Equals(CollectionItemCategory.All) || filter.CollectionItemCategory.Equals(CollectionItemCategory.Collections))
            {
                // Filter: Only children of the parent collection
                Expression<Func<Collection, bool>> filterExpression = c => c.ParentId == filter.CollectionId;
                // Order by Name
                Func<IQueryable<Collection>, IOrderedQueryable<Collection>> orderBy =
                    q => q.OrderBy(c => c.CreatedDate);
                // Include Marks (optional navigation property)
                var includes = new List<Expression<Func<Collection, object>>>
                {
                    c => c.Marks!
                };

                collections = await unitOfWork.collectionRepository
                    .GetAsyncPaged(
                        filter.Page,
                        filter.PageSize,
                        filterExpression,
                        orderBy,
                        includes
                    );

                // Map collection items
                items.AddRange(mapper.Map<List<CollectionItem>>(collections));
            }

            // Check if the limit of items is already supplied.
            if (collections.Count == filter.PageSize) return items;
            // Otherwise, adjust the limit
            filter.PageSize -= collections.Count;

            // Get children marks
            if (filter.CollectionItemCategory.Equals(CollectionItemCategory.All) || filter.CollectionItemCategory.Equals(CollectionItemCategory.Marks))
            {
                // Filter: Only children of the parent collection
                Expression<Func<Mark, bool>> filterExpression = c => c.CollectionId == filter.CollectionId;
                // Order by Name
                Func<IQueryable<Mark>, IOrderedQueryable<Mark>> orderBy =
                    q => q.OrderBy(c => c.CreatedDate);
                // Include Blocks
                var includes = new List<Expression<Func<Mark, object>>>
                {
                    c => c.Blocks!
                };

                marks = await unitOfWork.markRepository
                    .GetAsyncPaged(
                        filter.Page,
                        filter.PageSize,
                        filterExpression,
                        orderBy,
                        includes
                    );

                // Map mark items
                items.AddRange(mapper.Map<List<CollectionItem>>(marks));
            }

            return items;
        }
    }
}
