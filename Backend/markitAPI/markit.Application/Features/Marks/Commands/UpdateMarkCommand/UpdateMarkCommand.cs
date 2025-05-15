using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.UpdateMarkCommand
{
    public class UpdateMarkCommand : IRequest<MarkViewModel>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<BlockViewModel> Blocks { get; set; } = new();
    }

    public class UpdateMarkCommandHandler : IRequestHandler<UpdateMarkCommand, MarkViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateMarkCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(UpdateMarkCommand request, CancellationToken cancellationToken)
        {
            // Validate the existence of the mark
            Mark mark = await _unitOfWork.markRepository.GetByIdAsync(request.Id, "Blocks,Collection")
                ?? throw new NotFoundException("Mark", request.Id);

            // Mapping the request to the mark entity
            _mapper.Map(request, mark, typeof(UpdateMarkCommand), typeof(Mark));

            // Update the mark
            _unitOfWork.markRepository.UpdateEntity(mark);
            
            // Ordening Blocks
            mark.Blocks = mark.Blocks?.OrderBy(b => b.Order).ToList();

            // Complete the transaction
            await _unitOfWork.Complete();

            return _mapper.Map<MarkViewModel>(mark);
        }
    }
}
