using markit.Application.Common.Helpers;
using markit.Application.Common.Helpers.Services;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using MediatR;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery
{
    public class GetCollectionItemsPagedQuery(GetCollectionItemsPagedQueryDto dto, int creatorId) : IRequest<CollectionItemPage>
    {
        public CollectionItemPageRequest PaginationRequest = new()
        {
            CreatorId = creatorId,
            PageSize = dto.PageSize,
            Cursor = dto.Cursor,
            SortOrder = dto.SortOrder,
            Filters = dto.Filters,
        };
    }

    public class GetCollectionItemsPagedQueryHandler : IRequestHandler<GetCollectionItemsPagedQuery, CollectionItemPage>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CollectionItemService _collectionItemService;

        public GetCollectionItemsPagedQueryHandler(IUnitOfWork unitOfWork, CollectionItemService collectionItemService)
        {
            _unitOfWork = unitOfWork;
            _collectionItemService = collectionItemService;
        }

        public async Task<CollectionItemPage> Handle(GetCollectionItemsPagedQuery request, CancellationToken cancellationToken)
        {
            CollectionItemPageRequest paginationRequest = request.PaginationRequest;
            await ValidateCreator(paginationRequest.CreatorId);

            if (paginationRequest.Filters.CollectionId.HasValue) {
                await  ValidateCollection(paginationRequest.Filters.CollectionId.GetValueOrDefault(), paginationRequest.CreatorId);
            }

            return await GetItemsAsync(paginationRequest);
        }

        private async Task ValidateCreator(int creatorId)
        {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task ValidateCollection(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateCreator(creatorId);
        }

        private async Task<CollectionItemPage> GetItemsAsync(CollectionItemPageRequest paginationRequest)
        {
            return await _collectionItemService.GetItemsPageAsync(paginationRequest);
        }
    }
}
