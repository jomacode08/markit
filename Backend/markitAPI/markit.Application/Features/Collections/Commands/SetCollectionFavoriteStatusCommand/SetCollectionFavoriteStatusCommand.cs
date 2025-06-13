using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.SetCollectionFavoriteStatusCommand
{
    public class SetCollectionFavoriteStatusCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
        public bool IsFavorite { get; set; }
    }

    public class SetCollectionFavoriteStatusCommandHandler : IRequestHandler<SetCollectionFavoriteStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SetCollectionFavoriteStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SetCollectionFavoriteStatusCommand request, CancellationToken cancellationToken)
        {
            var collection = await ValidateCollection(request.Id, request.CreatorId);

            collection.IsFavorite = request.IsFavorite;
            _unitOfWork.collectionRepository.UpdateEntity(collection);
            await _unitOfWork.Complete();

            return collection.IsFavorite;
        }

        private async Task<Collection> ValidateCollection(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);

            if (collection.CreatorId != creatorId) throw new UnauthorizedAccessException();

            return collection;
        }
    }
}
