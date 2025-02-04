using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.CreateMarkCommand
{
    public class CreateMarkCommand : IRequest<MarkViewModel>
    {
        public string Name { get; set; } = string.Empty;
        public int CreatorId { get; set; }
        public List<BlockViewModel> Blocks { get; set; } = new();
    }

    public class CreateMarkCommandHandler : IRequestHandler<CreateMarkCommand, MarkViewModel>
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public CreateMarkCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(CreateMarkCommand request, CancellationToken cancellationToken)
        {
            // Validate the existence of the creator
            Creator? creator = await _unitOfWork.creatorRepository.GetByIdAsync(request.CreatorId);
            if (creator == null) throw new NotFoundException("Creator", request.CreatorId);

            // Map the request to a mark entity
            Mark mark = _mapper.Map<Mark>(request);
            // Create the mark
            _unitOfWork.markRepository.AddEntity(mark);

            // Complete the transaction
            await _unitOfWork.Complete();
            
            return _mapper.Map<MarkViewModel>(mark);
        }
    }
}
