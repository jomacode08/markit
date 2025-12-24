using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Queries
{
    public class GetMarkByIdQuery(int id, int creatorId) : IRequest<MarkViewModel>
    {
        public int Id { get; set; } = id;
        public int CreatorId {  get; set; } = creatorId;
    }

    public class GetMarkByIdQueryHandler : IRequestHandler<GetMarkByIdQuery, MarkViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMarkByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(GetMarkByIdQuery request, CancellationToken cancellationToken)
        {
            Mark? mark = await _unitOfWork.markRepository.GetWithOrderedBlocks(request.Id)
                ?? throw new NotFoundException("Mark", request.Id);
            mark.ValidateCreator(request.CreatorId);
            return _mapper.Map<MarkViewModel>(mark);
        }
    }
}
