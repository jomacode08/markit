using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Features.Collections.Queries.GetMainCollectionByCreator
{
    public class GetMainCollectionByCreatorQuery(int creatorId) : IRequest<CollectionViewModel>
    {
       public int CreatorId { get; set; } = creatorId;
    }

    public class GetMainCollectionByCreatorQueryHandler : IRequestHandler<GetMainCollectionByCreatorQuery, CollectionViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetMainCollectionByCreatorQuery> _logger;

        public GetMainCollectionByCreatorQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetMainCollectionByCreatorQuery> logger
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CollectionViewModel> Handle(GetMainCollectionByCreatorQuery request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistence(request.CreatorId);

            var mainCollection = await GetMainCollection(request.CreatorId);
            return MapCollection(mainCollection);
        }

        private async Task ValidateCreatorExistence(int creatorId)
        {
            _ = await _unitOfWork.CreatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<Collection> GetMainCollection(int creatorId)
        {
            var result = await _unitOfWork.CollectionRepository.GetAsync(c => c.CreatorId == creatorId && c.IsMain)
                ?? throw new CustomValidationException($"The main collection of the creator with ID: {creatorId} must be configured");

            return result[0];
        }

        private CollectionViewModel MapCollection(Collection collection)
        {
            CollectionViewModel collectionVm = _mapper.Map<CollectionViewModel>(collection);

            // Add path
            try
            {
                collectionVm.Path = Utilities.CreateCollectionPath(collection);
            }
            catch (FormatException ex)
            {
                _logger.LogError("{message}", ex.Message);
            }

            return collectionVm;
        }
    }
}
