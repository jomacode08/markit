using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.SetMarkFavoriteStatusCommand
{
    public class SetMarkFavoriteStatusCommand(int id, int creatorId, bool isFavorite) : IRequest<bool>
    {
        public int Id { get; set; } = id;
        public int CreatorId { get; set; } = creatorId;
        public bool IsFavorite { get; set; } = isFavorite;
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
            _unitOfWork.MarkRepository.UpdateEntity(mark);
         
            await _unitOfWork.Complete();
            return mark.IsFavorite;
        }

        private async Task<Mark> ValidateMark(int markId, int creatorId)
        {
            var mark = await _unitOfWork.MarkRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);
            mark.ValidateCreator(creatorId);
            return mark;
        }
    }
}
