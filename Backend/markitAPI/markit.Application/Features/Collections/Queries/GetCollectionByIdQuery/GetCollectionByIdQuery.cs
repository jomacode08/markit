using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Features.Collections.Queries.GetCollectionByIdQuery
{
    public class GetCollectionByIdQuery(int id, int creatorId) : IRequest<CollectionViewModel>
    {
        public int CollectionId { get; set; } = id;
        public int CreatorId { get; set; } = creatorId;
    }

    public class GetCollectionItemsQueryHandler : IRequestHandler<GetCollectionByIdQuery, CollectionViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCollectionByIdQuery> _logger;

        public GetCollectionItemsQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetCollectionByIdQuery> logger
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CollectionViewModel> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistence(request.CreatorId);
            var collection = await ValidateCollection(request.CollectionId, request.CreatorId);

            return MapCollection(collection);
        }

        private async Task<Collection> ValidateCollection(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateCreator(creatorId);
            return collection;
        }

        private async Task ValidateCreatorExistence(int creatorId)
        {
            _ = await _unitOfWork.CreatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private CollectionViewModel MapCollection(Collection collection)
        {
            CollectionViewModel collectionVm = _mapper.Map<CollectionViewModel>(collection);

            // Add path
            try
            {
                collectionVm.Path = collection.CreatePath();
            }
            catch (FormatException ex) {
                _logger.LogError(ex, "Failed to create path for collection {CollectionId}", collection.Id);
            }

            return collectionVm;
        }
    }
}
