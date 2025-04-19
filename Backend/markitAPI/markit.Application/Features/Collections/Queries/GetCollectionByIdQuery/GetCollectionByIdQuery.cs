using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsForGridQuery
{
    public class GetCollectionByIdQuery : IRequest<CollectionViewModel>
    {
        public int CollectionId { get; set; }
        public int CreatorId { get; set; }

        public bool IncludeCollectionITems { get; set; }
    }

    public class GetCollectionItemsQueryHandler : IRequestHandler<GetCollectionByIdQuery, CollectionViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCollectionItemsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CollectionViewModel> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
        {
            // Validations
            await ValidateCreatorExistency(request.CreatorId);
            var collection = await ValidateCollection(request.CollectionId, request.CreatorId);

            // Mapping Collection to CollectionViewModel
            CollectionViewModel collectionVm = _mapper.Map<CollectionViewModel>(collection);

            // Add CollectionItems if it's necesary
            if (request.IncludeCollectionITems)
            {
                collectionVm.CollectionItems = await _unitOfWork.collectionRepository.GetCollectionItems(request.CollectionId);
            }

            return collectionVm;
        }

        private async Task<Collection> ValidateCollection(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);

            if (collection.CreatorId != creatorId) throw new UnauthorizedAccessException();

            return collection;
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }
    }
}
