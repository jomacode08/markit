using System.Diagnostics;
using System.Transactions;
using AutoMapper;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.CreateCollectionCommand
{
    public class CreateCollectionCommand : IRequest<CollectionViewModel>
    {
        public string Name { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int CreatorId { get; set; }
        public int? ParentId { get; set; }

        public void Deconstruct( out string name,out int creatorId, out int? parentId )
        {
            name = Name;
            parentId = ParentId;
            creatorId = CreatorId;
        }
    }

    public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, CollectionViewModel>
    {
        private readonly IDocumentJobService<CollectionDocument> _documentJobService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCollectionCommandHandler(
            IDocumentJobService<CollectionDocument> documentJobService,
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _documentJobService = documentJobService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CollectionViewModel> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
        {
            // Validations
            await ValidateCreatorExistency(request.CreatorId);
            await ValidateNameDuplicates(request);
            if (request.IsMain) await ValidateMainCollectionDuplicate(request.CreatorId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                var collection = await CreateCollection(request);
                Collection? parent = collection.ParentId.HasValue ? await GetCollection((int)collection.ParentId) : null;
                collection = await UpdateCollectionPath(collection, parent);
                CreateDocumentBackgroundJob(collection);
                var collectionViewModel = _mapper.Map<CollectionViewModel>(collection);

            scope.Complete();
            return collectionViewModel;
        }

        private async Task<Unit> ValidateCreatorExistency(int creatorId)
        {
            var creator = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
            ?? throw new NotFoundException("Creator", creatorId);

            return Unit.Value;
        }

        private async Task<Unit> ValidateMainCollectionDuplicate(int creatorId)
        {
            var mainCollection = await _unitOfWork.collectionRepository
                .GetAsync(c => c.CreatorId.Equals(creatorId) && c.IsMain.Equals(true));

            if (mainCollection.Any()) throw new CustomValidationException("A main collection is already configured for the user");

            return Unit.Value;
        }

        private async Task<Unit> ValidateNameDuplicates(CreateCollectionCommand request)
        {
            (string name, int creatorId, int? parentId) = request;

            var duplicates = await _unitOfWork.collectionRepository
                .GetAsync(c =>
                    c.ParentId.Equals(parentId)
                    && c.Name.Equals(name)
                    && c.CreatorId.Equals(creatorId)
                );

            if (duplicates.Any()) throw new CustomValidationException(@$"There's already a collection with the name: { name }");

            return Unit.Value;
        }
        private async Task<Collection> GetCollection(int collectionId)
        {
            return await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
            ?? throw new NotFoundException("Collection", collectionId);
        }

        private async Task<Collection> CreateCollection(CreateCollectionCommand request)
        {
            var collection = _mapper.Map<Collection>(request);
            await _unitOfWork.collectionRepository.AddAsync(collection);
            return collection;
        }

        private void CreateDocumentBackgroundJob(Collection collection)
        {
            CollectionDocument document = new(collection.Name, collection.Id, collection.CreatorId);
            _documentJobService.ScheduleAddAsync(
                document,
                () => _unitOfWork.collectionRepository.UpdateSyncModelAsync(collection.Id, document.Id)
            );
        }

        private async Task<Collection> UpdateCollectionPath(Collection collection, Collection? parent)
        {
            collection.Path = CreatePathIds(collection.Id, parent);
            collection.PathNames = CreatePathNames(collection.Name, parent);
            return await _unitOfWork.collectionRepository.UpdateAsync(collection);
        }

        private static string CreatePathIds(int collectionId, Collection? parent = null)
        {
            if (parent == null) return @$"/{ collectionId }";

            return $@"{parent?.Path}/{collectionId}";
        }

        private static string CreatePathNames(string collectionName, Collection? parent = null)
        {
            if (parent == null) return @$"/{ collectionName }";

            return $@"{ parent?.PathNames }/{ collectionName }";
        }
    }
}
