using markit.Application.Common.Helpers.Services;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using MediatR;

namespace markit.Application.Features.Collections.Queries.GetCollectionChildrenPagedQuery
{
    public class GetCollectionChildrenPagedQuery : IRequest<CollectionItemPage>
    {
        public int CreatorId { get; set; }
        public int CollectionId {  get; set; }
        public int PageSize { get; set; }
        public required string? Cursor { get; set; }
        public CollectionItemFilter Filter { get; set; }
    }

    public class GetCollectionChildrenPagedQueryHandler : IRequestHandler<GetCollectionChildrenPagedQuery, CollectionItemPage>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CollectionItemService _collectionItemService;

        public GetCollectionChildrenPagedQueryHandler(IUnitOfWork unitOfWork, CollectionItemService collectionItemService)
        {
            _unitOfWork = unitOfWork;
            _collectionItemService = collectionItemService;
        }

        public async Task<CollectionItemPage> Handle(GetCollectionChildrenPagedQuery request, CancellationToken cancellationToken)
        {
            await ValidateCreator(request.CreatorId);
            await ValidateCollection(request.CollectionId, request.CreatorId);

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

            if (collection.CreatorId != creatorId) throw new UnauthorizedAccessException();
        }

        private async Task<CollectionItemPage> GetItemsAsync(GetCollectionChildrenPagedQuery request)
        {
            return await _collectionItemService.GetItemsPageAsync(
                new CollectionItemPageRequest(
                    request.CollectionId,
                    request.PageSize,
                    request.Filter,
                    request.Cursor
                )
            );
        }
    }
}
