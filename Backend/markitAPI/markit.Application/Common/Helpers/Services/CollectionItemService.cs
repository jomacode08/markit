using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;
using MediatR;
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

            if (IsSearchValidForItemType(
                itemType: CollectionItemType.Collection,
                filter: request.Filters.Type,
                cursor: cursorData
            ))
            {
                var collections = await _unitOfWork.CollectionRepository
                    .GetAsyncCursorBasedPagination(
                        request.PageSize,
                        request.CreatorId,
                        cursor: cursorData,
                        request.SortOrder,
                        request.Filters.CollectionId,
                        request.Filters.OnlyFavorites
                    );

                if (collections.Count < request.PageSize) cursorData = null;
                items.AddRange(_mapper.Map<List<CollectionItem>>(collections));
            }

            if (IsSearchValidForItemType(
                itemType: CollectionItemType.Mark,
                filter: request.Filters.Type,
                cursor: cursorData
            ))
            {
                var marks = await _unitOfWork.MarkRepository
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

            bool hasNextPage = items.Count > request.PageSize;
            items = [.. items.Take(request.PageSize)];

            if (items.Count.Equals(request.PageSize))
            {
                var last = items.Last();
                nextCursor = GenerateCursor(last);
            }

            return new CollectionItemPage(nextCursor, items, hasNextPage);
        }

        private static bool IsCursorValidForItemType(CollectionItemType type, CursorData cursor) => cursor.Type.Equals(type);

        private static bool IsSearchValidForItemType(CollectionItemType itemType, CollectionItemTypeFilter filter, CursorData? cursor)
        {
            CollectionItemTypeFilter expectedFilter = itemType switch
            {
                CollectionItemType.Collection => CollectionItemTypeFilter.Collection,
                CollectionItemType.Mark => CollectionItemTypeFilter.Mark,
                _ => throw new NotImplementedException(),
            };

            return filter.Equals(expectedFilter) || (filter.Equals(CollectionItemTypeFilter.All) && (cursor is null || IsCursorValidForItemType(itemType, cursor)));
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
    }
}
