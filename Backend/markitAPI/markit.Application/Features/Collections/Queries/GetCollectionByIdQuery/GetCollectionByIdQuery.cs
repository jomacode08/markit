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
    public class GetCollectionByIdQuery : IRequest<CollectionViewModel>
    {
        public int CollectionId { get; set; }
        public int CreatorId { get; set; }
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
            await ValidateCreatorExistency(request.CreatorId);
            var collection = await ValidateCollection(request.CollectionId, request.CreatorId);

            return MapCollection(collection);
        }

        private async Task<Collection> ValidateCollection(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateCreator(creatorId);
            return collection;
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private CollectionViewModel MapCollection(Collection collection)
        {
            CollectionViewModel collectionVm = _mapper.Map<CollectionViewModel>(collection);

            // Add path
            try
            {
                collectionVm.Path = Utilities.CreateCollectionPath(collection);
            }
            catch (FormatException ex) {
                _logger.LogError(ex.Message, ex);
            }

            return collectionVm;
        }
    }
}
