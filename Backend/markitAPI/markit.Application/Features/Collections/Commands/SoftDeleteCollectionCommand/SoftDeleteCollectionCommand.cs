using markit.Application.Common.Helpers;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Collections.Commands.DeleteCollectionCommand
{
    public class SoftDeleteCollectionCommand(int id, int creatorId) : IRequest<bool>
    {
        public int CollectionId { get; set; } = id;
        public int CreatorId { get; set; } = creatorId;
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
            var collection = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateCreator(creatorId);
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

        private async Task CreateDocumentBackgroundJob(Collection collection)
        {
            if (collection.DocumentId == null) return;

            CollectionDocument document = await _documentRepository.GetByIdAsync(collection.DocumentId);
            document.Enabled = false;

            _documentJobService.ScheduleUpdateAsync(
                document,
                () => _unitOfWork.CollectionRepository.UpdateSyncModelAsync(collection.Id, document.Id)
            );
        }
    }
}
