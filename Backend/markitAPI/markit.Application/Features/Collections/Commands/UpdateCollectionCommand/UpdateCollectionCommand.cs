using System.Transactions;
using AutoMapper;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.UpdateCollectionCommand
{
    public class UpdateCollectionCommand : IRequest<CollectionViewModel>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public int CreatorId { get; set; }
    }

    public class UpdateCollectionCommandHandler : IRequestHandler<UpdateCollectionCommand, CollectionViewModel>
    {
        private readonly IDocumentJobService<CollectionDocument> _documentJobService;
        private readonly IDocumentRepository<CollectionDocument> _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateCollectionCommandHandler(
            IDocumentJobService<CollectionDocument> documentJobService,
            IDocumentRepository<CollectionDocument> documentRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _documentJobService = documentJobService;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CollectionViewModel> Handle(UpdateCollectionCommand request, CancellationToken cancellationToken)
        {
            Collection collection = await ValidateCollection(request.Id, request.CreatorId);
            await ValidateNameDuplicates(request.Name, request.Id, collection.ParentId);
            int level = GetCollectionLevel(collection.PathNames);
            string newName = request.Name;

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                // Map and update collection
                _mapper.Map(request, collection, typeof(UpdateCollectionCommand), typeof(Collection));
                collection.PathNames = GetNewPath(nodeLevelToReplace: level, newName, oldPath: collection.PathNames);
                _unitOfWork.collectionRepository.UpdateEntity(collection);

                // Update descendants path
                List<Collection> hierarchy = await GetHierarchy(collection.Id);
                if (hierarchy.Count > 1)
                {
                    List<Collection> descendants = [.. hierarchy.Where(c => c.Id != collection.Id)];
                    UpdateDescendantsPath(level, newName, descendants);
                }

                await _unitOfWork.Complete(); //Save context changes
                await CreateDocumentBackgroundJob(collection);
                var collectionViewModel = _mapper.Map<CollectionViewModel>(collection);

            scope.Complete();// Complete transaction
            return collectionViewModel;
        }

        private async Task ValidateNameDuplicates(string name, int collectionId, int? parentId)
        {
            var duplicates = await _unitOfWork.collectionRepository
                .GetAsync(c => 
                    c.ParentId.Equals(parentId)
                    && c.Name.Equals(name) 
                    && c.Id != collectionId
                );

            if (duplicates.Any()) throw new CustomValidationException(@$"There's already a collection with the name: {name}");
        }

        private async Task<Collection> ValidateCollection(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);

            if (collection.CreatorId != creatorId) throw new UnauthorizedAccessException();

            return collection;
        }
        private void UpdateDescendantsPath(int level, string newName, List<Collection> descendants)
        {
            foreach (Collection descendant in descendants)
            {
                descendant.PathNames = GetNewPath(nodeLevelToReplace: level, newName, oldPath: descendant.PathNames);
                _unitOfWork.collectionRepository.UpdateEntity(descendant);
            }
        }

        private async Task<List<Collection>> GetHierarchy(int collectionId)
        {
            return await _unitOfWork.collectionRepository.GetHierarchyRecursively(collectionId);
        }

        private static string GetNewPath(int nodeLevelToReplace, string newNodeName, string oldPath)
        {
            string[]? pathSegments = [.. oldPath.Split('/').Where(s => s != "")];
            pathSegments.SetValue(newNodeName, nodeLevelToReplace - 1);

            return @$"/{ string.Join("/", pathSegments) }";
        }

        private static int GetCollectionLevel(string path)
        {
            return path.Split('/').Where(s => s != "").Count();
        }

        private async Task CreateDocumentBackgroundJob(Collection collection)
        {
            if (collection.DocumentId == null) return;

            CollectionDocument document = await _documentRepository.GetByIdAsync(collection.DocumentId);
            document.Name = collection.Name;

            _documentJobService.ScheduleUpdateAsync(
                document,
                () => _unitOfWork.collectionRepository.UpdateSyncModelAsync(collection.Id, document.Id)
            );
        }
    }
}
