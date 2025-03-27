using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Queries
{
    public class GetMarksByCreatorIdQuery : IRequest<List<MarkViewModel>>
    {
        public int CreatorId { get; set; }
    }

    public class GetMarksByCreatorIdHandler : IRequestHandler<GetMarksByCreatorIdQuery, List<MarkViewModel>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetMarksByCreatorIdHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<List<MarkViewModel>> Handle(GetMarksByCreatorIdQuery request, CancellationToken cancellationToken)
        {
            // Validate the existence of the creator
            Creator creator = await _unitOfWork.creatorRepository.GetByIdAsync(request.CreatorId)
                ?? throw new NotFoundException("Creator", request.CreatorId);

            // Get the marks by the creator
            var marks = await _unitOfWork.markRepository
                .GetAsync(m => m.Collection != null && m.Collection.CreatorId.Equals(request.CreatorId));

            return _mapper.Map<List<MarkViewModel>>(marks);
        }
    }
}
