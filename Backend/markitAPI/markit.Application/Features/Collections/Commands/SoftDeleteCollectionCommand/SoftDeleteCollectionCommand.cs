using System.Transactions;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.DeleteCollectionCommand
{
    public class SoftDeleteCollectionCommand : IRequest<bool>
    {
        public int CollectionId { get; set; }
        public int CreatorId { get; set; }
    }

    public class DeleteCollectionCommandHandler : IRequestHandler<SoftDeleteCollectionCommand, bool>
    {
        private readonly IDocumentJobService<CollectionDocument> _documentJobService;
        private readonly IDocumentRepository<CollectionDocument> _documentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteCollectionCommandHandler(
            IDocumentJobService<CollectionDocument> documentJobService,
            IDocumentRepository<CollectionDocument> documentRepository,
            IUnitOfWork unitOfWork
        )
        {
            _documentJobService = documentJobService;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SoftDeleteCollectionCommand request, CancellationToken cancellationToken)
        {
            var collection = await ValidateCollectionExistency(request.CollectionId, request.CreatorId);
            List<Collection> hierarchy = await GetHierarchy(collectionId: request.CollectionId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                await SoftDeleteOnCascade(hierarchy);
                await _unitOfWork.Complete();
                await CreateDocumentBackgroundJob(collection);
            scope.Complete();

            return true;
        }

        private async Task<Collection> ValidateCollectionExistency(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);

            if (collection.CreatorId != creatorId) throw new UnauthorizedAccessException();
            return collection;
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

        private async Task CreateDocumentBackgroundJob(Collection collection)
        {
            if (collection.DocumentId == null) return;

            CollectionDocument document = await _documentRepository.GetByIdAsync(collection.DocumentId);
            document.Enabled = false;

            _documentJobService.ScheduleUpdateAsync(
                document,
                () => _unitOfWork.collectionRepository.UpdateSyncModelAsync(collection.Id, document.Id)
            );
        }
    }
}
