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
    public class GetCollectionByIdQuery(int id, string userId) : IRequest<CollectionViewModel>
    {
        public int CollectionId { get; set; } = id;
        public string UserId { get; set; } = userId;
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
            Collection collection = await ValidateCollection(request.CollectionId, request.UserId);
            return MapCollection(collection);
        }

        private async Task<Collection> ValidateCollection(int collectionId, string userId)
        {
            var collection = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateUser(userId);
            return collection;
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
