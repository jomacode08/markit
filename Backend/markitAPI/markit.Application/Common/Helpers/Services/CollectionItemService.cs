using System.Globalization;
using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;

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
            bool hasNextPage = false;
            string? nextCursor = null;

            if (request.Filter.Equals(CollectionItemFilter.All) || request.Filter.Equals(CollectionItemFilter.Collection))
            {
                var collections = await _unitOfWork.collectionRepository
                    .GetAsyncCursorBasedPagination(request.CollectionId, request.PageSize, cursorData);

                items.AddRange(_mapper.Map<List<CollectionItem>>(collections));
            }

            if (request.Filter.Equals(CollectionItemFilter.All) || request.Filter.Equals(CollectionItemFilter.Mark))
            {
                var marks = await _unitOfWork.markRepository
                    .GetAsyncCursorBasedPagination(request.CollectionId, request.PageSize, cursorData);

                items.AddRange(_mapper.Map<List<CollectionItem>>(marks));
            }

            if (request.Filter.Equals(CollectionItemFilter.All))
            {
                items = [.. items.OrderBy(c => c.CreatedAt)
                    .ThenBy(c => c.Type)
                    .ThenBy(c => c.TypeId)
                    .Take(request.PageSize + 1)
                ];
            }

            hasNextPage = items.Count > request.PageSize;
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
    }
}
