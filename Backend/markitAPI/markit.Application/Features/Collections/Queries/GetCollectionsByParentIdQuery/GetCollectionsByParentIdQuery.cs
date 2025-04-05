using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using MediatR;

namespace markit.Application.Features.Collections.Queries.GetCollectionsByParentIdQuery
{
    public class GetCollectionsByParentIdQuery : IRequest<List<CollectionViewModel>>
    {
        public int ParentId { get; set; }
    }

    public class GetCollectionByParentIdQueryHandler : IRequestHandler<GetCollectionsByParentIdQuery, List<CollectionViewModel>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetCollectionByParentIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<CollectionViewModel>> Handle(GetCollectionsByParentIdQuery request, CancellationToken cancellationToken)
        {
            await ValidateParentCollectionExistency( parentCollectionId: request.ParentId);

            var collections = await _unitOfWork.collectionRepository.GetAsync(c => c.ParentId.Equals(request.ParentId));
            return _mapper.Map<List<CollectionViewModel>>(collections);
        }

        private async Task ValidateParentCollectionExistency(int parentCollectionId)
        {
            _ = await _unitOfWork.collectionRepository.GetByIdAsync(parentCollectionId)
                ?? throw new NotFoundException("Collection", parentCollectionId);
        }
    }
}
