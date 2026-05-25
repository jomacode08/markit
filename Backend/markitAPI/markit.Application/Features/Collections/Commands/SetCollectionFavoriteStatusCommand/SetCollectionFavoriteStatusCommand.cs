using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.SetCollectionFavoriteStatusCommand
{
    public class SetCollectionFavoriteStatusCommand(int id, string userId, bool isFavorite) : IRequest<bool>
    {
        public int Id { get; set; } = id;
        public string UserId { get; set; } = userId;
        public bool IsFavorite { get; set; } = isFavorite;
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
            var collection = await ValidateCollection(request.Id, request.UserId);

            collection.IsFavorite = request.IsFavorite;
            _unitOfWork.CollectionRepository.UpdateEntity(collection);
            await _unitOfWork.Complete();

            return collection.IsFavorite;
        }

        private async Task<Collection> ValidateCollection(int collectionId, string userId)
        {
            Collection collection = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateUser(userId);
            return collection;
        }
    }
}
