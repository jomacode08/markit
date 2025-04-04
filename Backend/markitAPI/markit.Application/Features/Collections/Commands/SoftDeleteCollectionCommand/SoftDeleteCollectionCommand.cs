using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.DeleteCollectionCommand
{
    public class SoftDeleteCollectionCommand : IRequest<bool>
    {
        public int CollectionId { get; set; }
    }

    public class DeleteCollectionCommandHandler : IRequestHandler<SoftDeleteCollectionCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCollectionCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SoftDeleteCollectionCommand request, CancellationToken cancellationToken)
        {
            await ValidateCollectionExistency(request.CollectionId);

            List<Collection> hierarchy = await GetHierarchy(collectionId: request.CollectionId);
            await SoftDeleteOnCascade(hierarchy);

            await _unitOfWork.Complete();
            return true;
        }

        private async Task<Unit> ValidateCollectionExistency(int collectionId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);

            return Unit.Value;
        }

        private async Task<List<Collection>> GetHierarchy(int collectionId)
        {
            return await _unitOfWork.collectionRepository.GetHierarchyRecursively(collectionId);
        }

        private async Task<List<Mark>> GetMarks(int[] ids)
        {
            return [.. await _unitOfWork.markRepository.GetAsync(m => ids.Contains(m.CollectionId))];
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
                    _unitOfWork.markRepository.SoftDeleteRangeEntity([.. collectionMarks]);
                }

                // Soft delete to collection
                _unitOfWork.collectionRepository.SoftDeleteEntity(collection);
            }

            return Unit.Value;
        }
    }
}
