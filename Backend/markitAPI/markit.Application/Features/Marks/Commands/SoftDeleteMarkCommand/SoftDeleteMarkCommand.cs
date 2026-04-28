using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Marks.Commands.DeleteMarkCommand
{
    public class SoftDeleteMarkCommand(int id, int creatorId) : IRequest<bool>
    {
        public int Id { get; set; } = id;
        public int CreatorId { get; set; } = creatorId;
    }

    public class SoftDeleteMarkCommandHandler : IRequestHandler<SoftDeleteMarkCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SoftDeleteMarkCommandHandler(
            IUnitOfWork unitOfWork
        )
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SoftDeleteMarkCommand request, CancellationToken cancellationToken)
        {
            Mark mark = await ValidateMarkExistence(request.Id, request.CreatorId);
            SoftDeleteMark(mark);
            await SoftDeleteBlocks(mark.Id);
            await _unitOfWork.Complete();
            return true;
        }

        private async Task<Mark> ValidateMarkExistence(int markId, int creatorId)
        {
            Mark mark = await _unitOfWork.MarkRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);
            mark.ValidateCreator(creatorId);
            return mark;
        }

        private void SoftDeleteMark(Mark mark)
        {
            _unitOfWork.MarkRepository.SoftDeleteEntity(mark);
        }

        private async Task SoftDeleteBlocks(int markId)
        {
            var blocks = await _unitOfWork.BlockRepository.GetAsync(b => b.MarkId.Equals(markId));
            _unitOfWork.BlockRepository.SoftDeleteRangeEntity([.. blocks]);
        }
    }
}
