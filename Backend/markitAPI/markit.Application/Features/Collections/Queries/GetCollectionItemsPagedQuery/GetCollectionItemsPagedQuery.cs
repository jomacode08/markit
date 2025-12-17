using markit.Application.Common.Exceptions;
using markit.Application.Common.Helpers;
using markit.Application.Common.Helpers.Services;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using MediatR;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery
{
    public class GetCollectionItemsPagedQuery : CollectionItemPageRequest, IRequest<CollectionItemPage>
    {
        public int CreatorId { get; set; }
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
            await ValidateCreator(request.CreatorId);

            if (request.Filters.CollectionId.HasValue) {
                await  ValidateCollection(request.Filters.CollectionId.GetValueOrDefault(), request.CreatorId);
            }

            return await GetItemsAsync(request);
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

        private async Task<CollectionItemPage> GetItemsAsync(GetCollectionItemsPagedQuery request)
        {
            return await _collectionItemService.GetItemsPageAsync(request);
        }
    }
}
