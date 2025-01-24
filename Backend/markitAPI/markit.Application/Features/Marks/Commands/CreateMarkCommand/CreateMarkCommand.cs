using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.CreateMarkCommand
{
    public class CreateMarkCommand : IRequest<Unit>
    {
        public string Name { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public int CreatorId { get; set; }
    }

    public class CreateMarkCommandHandler : IRequestHandler<CreateMarkCommand, Unit>
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public CreateMarkCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<Unit> Handle(CreateMarkCommand request, CancellationToken cancellationToken)
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
            return Unit.Value;
        }
    }
}
