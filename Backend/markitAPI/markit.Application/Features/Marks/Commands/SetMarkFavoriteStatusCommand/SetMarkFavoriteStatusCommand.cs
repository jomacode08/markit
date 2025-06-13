using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.SetMarkFavoriteStatusCommand
{
    public class SetMarkFavoriteStatusCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class SetMarkFavoriteStatusCommandHandler : IRequestHandler<SetMarkFavoriteStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SetMarkFavoriteStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SetMarkFavoriteStatusCommand request, CancellationToken cancellationToken)
        {
            var mark = await ValidateMark(request.Id, request.CreatorId);
            
            mark.IsFavorite = request.IsFavorite;
            _unitOfWork.markRepository.UpdateEntity(mark);
         
            await _unitOfWork.Complete();
            return mark.IsFavorite;
        }

        private async Task<Mark> ValidateMark(int markId, int creatorId)
        {
            var mark = await _unitOfWork.markRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);

            if (mark.Collection?.CreatorId != creatorId) throw new UnauthorizedAccessException();

            return mark;
        }
    }
}
