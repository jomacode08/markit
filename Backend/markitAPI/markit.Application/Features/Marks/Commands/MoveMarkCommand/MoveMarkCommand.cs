using markit.Application.Common.Exceptions;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.MoveMarkCommand
{
    public class MoveMarkCommand(int markId, int collectionId, int creatorId) : IRequest<Unit>
    {
        public int MarkId { get; init; } = markId;
        public int CollectionId { get; init; } = collectionId;
        public int CreatorId { get; init; } = creatorId;
    }

    public class MoveMarkCommandHandler : IRequestHandler<MoveMarkCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MoveMarkCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(MoveMarkCommand request, CancellationToken cancellationToken)
        {
            Mark mark = await GetMarkAsync(request.MarkId);
            Collection sourceCollection = await GetCollectionAsync(mark.CollectionId);
            Collection destinyCollection = await GetCollectionAsync(request.CollectionId);
            
            if (sourceCollection.CreatorId != request.CreatorId) throw new ForbiddenResourceException("Marks", mark.Id, request.CreatorId);
            if (destinyCollection.CreatorId != request.CreatorId) throw new ForbiddenResourceException("Collections", destinyCollection.Id, request.CreatorId);
            if (mark.CollectionId.Equals(request.CollectionId)) return Unit.Value;

            await MoveMarkAsync(mark, newCollectionId: destinyCollection.Id);
            return Unit.Value;
        }

        private async Task MoveMarkAsync(Mark mark, int newCollectionId)
        {
            mark.CollectionId = newCollectionId;
            await _unitOfWork.MarkRepository.UpdateAsync(mark);
        }

        private async Task<Collection> GetCollectionAsync(int collectionId)
        {
            return await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collections", collectionId);
        }

        private async Task<Mark> GetMarkAsync(int markId)
        {
            return await _unitOfWork.MarkRepository.GetByIdAsync(markId)
                ?? throw new NotFoundException("Marks", markId);
        }
    }
}
