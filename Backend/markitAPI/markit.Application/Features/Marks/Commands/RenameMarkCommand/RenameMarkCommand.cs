using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Marks.Commands.RenameMarkCommand
{
    public class RenameMarkCommand(int id, RenameMarkDto dto) : IRequest<MarkViewModel>
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = dto.Name;
        public string? Emoji { get; set; } = dto.Emoji;
        public int CreatorId { get; set; } = dto.CreatorId;
    }

    public class RenameMarkCommandHandler : IRequestHandler<RenameMarkCommand, MarkViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RenameMarkCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(RenameMarkCommand request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistency(request.CreatorId);
            var mark = await ValidateMarkExistency(request.Id, request.CreatorId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                mark.Name = request.Name;
                mark.Emoji = request.Emoji;
                await UpdateMarkAsync(mark);
                var markViewModel = _mapper.Map<MarkViewModel>(mark);
            scope.Complete();

            return markViewModel;
        }

        private async Task ValidateCreatorExistency(int creatorId) {
            _ = await _unitOfWork.CreatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<Mark> ValidateMarkExistency(int markId, int creatorId)
        {
            Mark? mark = await _unitOfWork.MarkRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);
            mark.ValidateCreator(creatorId);
            return mark;
        }

        private async Task UpdateMarkAsync(Mark mark) => await _unitOfWork.MarkRepository.UpdateAsync(mark);
    }
}
