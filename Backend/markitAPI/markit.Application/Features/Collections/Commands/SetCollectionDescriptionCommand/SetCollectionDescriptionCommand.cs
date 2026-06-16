using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.SetCollectionDescriptionCommand
{
    public class SetCollectionDescriptionCommand(int id, string description, string userId) : IRequest<Unit>
    {
        public int Id { get; private set; } = id;
        public string Description { get; private set; } = description;
        public string UserId { get; private set;} = userId;
    }

    public class SetCollectionDescriptionCommandHandler : IRequestHandler<SetCollectionDescriptionCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SetCollectionDescriptionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(SetCollectionDescriptionCommand request, CancellationToken cancellationToken)
        {
            Collection collection = await ValidateCollectionAsync(request.Id, request.UserId);
            collection.Description = request.Description;
            await _unitOfWork.CollectionRepository.UpdateAsync(collection);
            return Unit.Value;
        }

        private async Task<Collection> ValidateCollectionAsync(int id, string userId)
        {
            Collection collection = await _unitOfWork.CollectionRepository.GetByIdAsync(id)
                ?? throw new NotFoundException("Collection", id);
            collection.ValidateUser(userId);
            return collection;
        }
    }
}
