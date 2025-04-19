using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.RenameMarkCommand
{
    public class RenameMarkCommand : IRequest<MarkViewModel>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public int CreatorId { get; set; }
    }

    public class RenameMarkCommandHandler : IRequestHandler<RenameMarkCommand, MarkViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RenameMarkCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(RenameMarkCommand request, CancellationToken cancellationToken)
        {
            // Validations
            await ValidateCreatorExistency(request.CreatorId);
            var mark = await ValidateMarkExistency(request.Id, request.CreatorId);
            
            // Update mark
            mark.Name = request.Name;
            _unitOfWork.markRepository.UpdateEntity(mark);

            // Complete transaction
            await _unitOfWork.Complete();
            return _mapper.Map<MarkViewModel>(mark);
        }

        private async Task ValidateCreatorExistency(int creatorId) {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<Mark> ValidateMarkExistency(int markId, int creatorId)
        {
            Mark? mark = await _unitOfWork.markRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);

            if (mark.Collection?.CreatorId != creatorId) throw new UnauthorizedAccessException();
            return mark;
        }
    }
}
