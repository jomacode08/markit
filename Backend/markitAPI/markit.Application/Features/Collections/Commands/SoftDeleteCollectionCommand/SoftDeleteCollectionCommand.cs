using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.DeleteCollectionCommand
{
    public class SoftDeleteCollectionCommand(int id, string userId) : IRequest<bool>
    {
        public int CollectionId { get; set; } = id;
        public string UserId { get; set; } = userId;
    }

    public class DeleteCollectionCommandHandler : IRequestHandler<SoftDeleteCollectionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCollectionCommandHandler(
            IUnitOfWork unitOfWork
        )
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SoftDeleteCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await ValidateCollection(request.CollectionId, request.UserId);
            List<Collection> hierarchy = await GetHierarchy(collectionId: request.CollectionId);
            await SoftDeleteOnCascade(hierarchy);
            await _unitOfWork.Complete();
            return true;
        }

        private async Task<Collection> ValidateCollection(int collectionId, string userId)
        {
            Collection collection = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateUser(userId);
            return collection;
        }

        private async Task<List<Collection>> GetHierarchy(int collectionId)
        {
            return await _unitOfWork.CollectionRepository.GetHierarchyRecursively(collectionId);
        }

        private async Task<List<Mark>> GetMarks(int[] ids)
        {
            return [.. await _unitOfWork.MarkRepository.GetAsync(m => ids.Contains(m.CollectionId))];
        }

        private async Task<Unit> SoftDeleteOnCascade(List<Collection> collections)
        {
            int[] collectionIds = [.. collections.Select(c => c.Id)];
            var marks = await GetMarks(collectionIds);

            foreach (Collection collection in collections)
            {
                if (collection.IsMain) throw new CustomValidationException($@"The main collection '{ collection.Name }' can't be deleted.");

                // Get marks related to current collection
                var collectionMarks = marks.Where(m => m.CollectionId.Equals(collection.Id));

                // Soft delete to related marks
                if (collectionMarks.Any())
                {
                    _unitOfWork.MarkRepository.SoftDeleteRangeEntity([.. collectionMarks]);
                }

                // Soft delete to collection
                _unitOfWork.CollectionRepository.SoftDeleteEntity(collection);
            }

            return Unit.Value;
        }
    }
}
