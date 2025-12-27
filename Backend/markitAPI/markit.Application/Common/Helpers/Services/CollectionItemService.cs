using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery;
using markit.Application.Features.Collections.Queries.ViewModels;
using System.Globalization;

namespace markit.Application.Common.Helpers.Services
{
    public class CollectionItemService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        private readonly IUnitOfWork _unitOfWork = unitOfWork;
        private readonly IMapper _mapper = mapper;

        public async Task<CollectionItemPage> GetItemsPageAsync(CollectionItemPageRequest request)
        {
            List<CollectionItem> items = [];
            CursorData? cursorData = ParseCursor(request.Cursor);
            string? nextCursor = null;

            if (request.Filters.Type.Equals(CollectionItemTypeFilter.All) || request.Filters.Type.Equals(CollectionItemTypeFilter.Collection))
            {
                var collections = await _unitOfWork.collectionRepository
                    .GetAsyncCursorBasedPagination(
                        request.PageSize,
                        request.CreatorId,
                        cursor: cursorData,
                        request.SortOrder,
                        request.Filters.CollectionId,
                        request.Filters.OnlyFavorites
                    );

                items.AddRange(_mapper.Map<List<CollectionItem>>(collections));
            }

            if (request.Filters.Type.Equals(CollectionItemTypeFilter.All) || request.Filters.Type.Equals(CollectionItemTypeFilter.Mark))
            {
                var marks = await _unitOfWork.markRepository
                    .GetAsyncCursorBasedPagination(
                        request.PageSize,
                        request.CreatorId,
                        cursor: cursorData,
                        request.SortOrder,
                        request.Filters.CollectionId,
                        request.Filters.OnlyFavorites
                    );

                items.AddRange(_mapper.Map<List<CollectionItem>>(marks));
            }

            if (request.Filters.Type.Equals(CollectionItemTypeFilter.All))
            {

                var orderedItems = OrderItems(request.SortOrder, items);
                items = [.. orderedItems.Take(request.PageSize + 1)];
            }

            bool hasNextPage = items.Count > request.PageSize;
            items = [.. items.Take(request.PageSize)];

            if (items.Count.Equals(request.PageSize))
            {
                var last = items.Last();
                nextCursor = GenerateCursor(last);
            }

            return new CollectionItemPage(nextCursor, items, hasNextPage);
        }

        private static string GenerateCursor(CollectionItem lastItem) => $"{lastItem.CreatedAt:O}|{Enum.GetName(typeof(CollectionItemType), lastItem.Type)}|{lastItem.TypeId}";

        private static CursorData? ParseCursor(string? cursor)
        {
            if (string.IsNullOrWhiteSpace(cursor)) return null;

            var parts = cursor.Split('|');

            if (parts.Length != 3) return null;
            if (!DateTime.TryParse(parts[0], null, DateTimeStyles.RoundtripKind, out DateTime createdAt)) return null;
            if (!Enum.TryParse(parts[1], true, out CollectionItemType type)) return null;
            if (!int.TryParse(parts[2], out int id)) return null;

            return new CursorData(createdAt, type, id);
        }

        private static IOrderedEnumerable<CollectionItem> OrderItems(SortPaginationOrder sortOrder, List<CollectionItem> items)
        {
            if (sortOrder.Equals(SortPaginationOrder.Ascending))
            {
                return items
                    .OrderBy(c => c.CreatedAt)
                    .ThenBy(c => c.Type)
                    .ThenBy(c => c.TypeId);
            }
            else
            {
                return items
                    .OrderByDescending(c => c.CreatedAt)
                    .ThenByDescending(c => c.Type)
                    .ThenByDescending(c => c.TypeId);
            }
        }
    }
}
