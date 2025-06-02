using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Common.Helpers.Services;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Helpers;
using markit.Application.Models.Filters;
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
        private readonly CollectionItemService _collectionItemService;

        public GetMainCollectionByCreatorQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetMainCollectionByCreatorQuery> logger,
            CollectionItemService collectionItemService
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
            _collectionItemService = collectionItemService;
        }

        public async Task<CollectionViewModel> Handle(GetMainCollectionByCreatorQuery request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistency(request.CreatorId);

            var mainCollection = await GetMainCollection(request.CreatorId);
            return await MapCollection(mainCollection);
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

        private async Task<List<CollectionItem>> GetCollectionItems(CollectionItemPagedFilter filter)
        {
            return await _collectionItemService.GetCollectionItemsPaged(filter);
        }

        private async Task<CollectionViewModel> MapCollection(Collection collection)
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

            // Get collection items
            collectionVm.CollectionItems = await GetCollectionItems(new CollectionItemPagedFilter
            {
                CollectionId = collection.Id,
                Page = 1,
                PageSize = GeneralConstant.Configuration.DEFAULT_PAGINATION_PAGE_SIZE,
                CollectionItemCategory = CollectionItemCategory.All
            });

            return collectionVm;
        }
    }
}
