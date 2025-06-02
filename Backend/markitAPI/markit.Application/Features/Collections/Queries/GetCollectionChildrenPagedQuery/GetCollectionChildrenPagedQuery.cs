using markit.Application.Common.Helpers.Services;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Models.Filters;
using MediatR;

namespace markit.Application.Features.Collections.Queries.GetCollectionChildrenPagedQuery
{
    public class GetCollectionChildrenPagedQuery : CollectionItemPagedFilter, IRequest<List<CollectionItem>>
    {
        public int CreatorId { get; set; }
    }

    public class GetCollectionChildrenPagedQueryHandler : IRequestHandler<GetCollectionChildrenPagedQuery, List<CollectionItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CollectionItemService _collectionItemService;

        public GetCollectionChildrenPagedQueryHandler(IUnitOfWork unitOfWork, CollectionItemService collectionItemService)
        {
            _unitOfWork = unitOfWork;
            _collectionItemService = collectionItemService;
        }

        public async Task<List<CollectionItem>> Handle(GetCollectionChildrenPagedQuery request, CancellationToken cancellationToken)
        {
            await ValidateCreator(request.CreatorId);
            await ValidateCollection(request.CollectionId, request.CreatorId);

            return await MapCollectionItems(request);
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

        private async Task<List<CollectionItem>> MapCollectionItems(GetCollectionChildrenPagedQuery request)
        {
            return await _collectionItemService.GetCollectionItemsPaged(request);
        }
    }
}
