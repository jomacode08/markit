using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Features.Collections.Queries.GetMainCollectionByCreatorQuery
{
    public class GetMainCollectionByCreatorQuery : IRequest<CollectionViewModel>
    {
       public int CreatorId { get; set; }
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
            await ValidateCreatorExistency(request.CreatorId);

            var mainCollection = await GetMainCollection(request.CreatorId);
            return MapCollection(mainCollection);
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<Collection> GetMainCollection(int creatorId)
        {
            var result = await _unitOfWork.collectionRepository.GetAsync(c => c.CreatorId == creatorId && c.IsMain)
                ?? throw new CustomValidationException($"The main collection of the creator with ID: {creatorId} must be configurated");

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
                _logger.LogError(ex.Message, ex);
            }

            return collectionVm;
        }
    }
}
