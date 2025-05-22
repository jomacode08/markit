using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Queries.Grid
{
    public class GetRootCollectionsForGridQuery : IRequest<List<CollectionItem>>
    {
        public int CreatorId { get; set; }
    }

    public class GetRootCollectionsForGridQueryHandler : IRequestHandler<GetRootCollectionsForGridQuery, List<CollectionItem>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetRootCollectionsForGridQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<CollectionItem>> Handle(GetRootCollectionsForGridQuery request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistency(request.CreatorId);
            var collections = await GetRootCollections(request.CreatorId);

            return _mapper.Map<List<CollectionItem>>(collections);
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<List<Collection>> GetRootCollections(int creatorId)
        {
            return [.. await _unitOfWork.collectionRepository
                .GetAsync(c => c.CreatorId.Equals(creatorId) && c.ParentId.Equals(null), null, "Marks")
            ];
        }
    }
}
