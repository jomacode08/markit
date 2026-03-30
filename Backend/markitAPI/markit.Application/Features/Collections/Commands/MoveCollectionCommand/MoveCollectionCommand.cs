﻿using markit.Application.Common.Exceptions;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Features.Collections.Commands.MoveCollectionCommand
{
    public class MoveCollectionCommand(int collectionId, int parentId, int creatorId) : IRequest<Unit>
    {
        public int CollectionId { get; init; } = collectionId;
        public int ParentId { get; init; } = parentId;
        public int CreatorId { get; init; } = creatorId;
    }

    public record CollectionPath(string Ids, string Names);

    public class MoveCollectionCommandHandler : IRequestHandler<MoveCollectionCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<MoveCollectionCommandHandler> _logger;
        private const string MAIN_COLLECTION_ERROR_MESSAGE = "The main collection can't be moved.";
        private const string DESCENDANT_PARENT_CASE_ERROR_MESSAGE = "The collection can't be moved to one of its descendants";
        private const string MISSING_COLLECTION_PATH_ERROR_MESSAGE = "The 'Path' property of the collection is required.";
        private const string BAD_CONFIGURATION_ERROR_MESSAGE = "There is a configuration error in the collection structure, the operation can't proceed.";

        public MoveCollectionCommandHandler(IUnitOfWork unitOfWork, ILogger<MoveCollectionCommandHandler> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<Unit> Handle(MoveCollectionCommand request, CancellationToken cancellationToken)
        {
            Collection collection = await GetCollection(request.CollectionId);
            Collection parent = await GetCollection(request.ParentId);

            if (collection.CreatorId != request.CreatorId) throw new ForbiddenResourceException("Collection", collection.Id, request.CreatorId);
            if (parent.CreatorId != request.CreatorId) throw new ForbiddenResourceException("Collection", parent.Id, request.CreatorId);
            if (collection.IsMain) throw new CustomValidationException(MAIN_COLLECTION_ERROR_MESSAGE);
            if (collection.Path is null) throw HandleMissingPathError(collection.Id);
            if (parent.Path is null) throw HandleMissingPathError(parent.Id);
            if (collection.Id.Equals(parent.Id)) return Unit.Value;
            if (collection.ParentId == parent.Id) return Unit.Value;
            
            IEnumerable<Collection> descendants = await GetCollectionDescendants(collection.Id);
            ValidateDescendantParentCase(descendants, parent.Id);

            CollectionPath parentPath = new(
                Ids: parent.Path,
                Names: parent.PathNames
            );
            CollectionPath oldPath = new(
                Ids: collection.Path,
                Names: collection.PathNames
            );
            CollectionPath newPath = new(
                Ids: $"{parentPath.Ids}/{collection.Id}",
                Names: $"{parentPath.Names}/{collection.Name}"
            );
            
            UpdateCollectionInMemory(
                collection,
                parent.Id,
                newPath
            );
            UpdateDescendantsInMemory(
                descendants,
                oldPath,
                newPath
            );

            await _unitOfWork.Complete();
            return Unit.Value;
        }

        private void UpdateDescendantsInMemory(
            IEnumerable<Collection> descendants,
            CollectionPath oldPath,
            CollectionPath newPath
        )
        {
            foreach (Collection descendant in descendants)
            {
                if (descendant.Path is null) throw HandleMissingPathError(descendant.Id);
                descendant.Path = descendant.Path.Replace(oldPath.Ids, newPath.Ids);
                descendant.PathNames = GenerateDescendantNamePath(
                    path: descendant.PathNames,
                    oldPath: oldPath.Names,
                    newPath: newPath.Names
                );

                _unitOfWork.CollectionRepository.UpdateEntity(descendant);
            }
        }

        private void UpdateCollectionInMemory(Collection collection, int parentId, CollectionPath path)
        {
            collection.ParentId = parentId;
            collection.Path = path.Ids;
            collection.PathNames = path.Names;
            _unitOfWork.CollectionRepository.UpdateEntity(collection);
        }

        private async Task<IEnumerable<Collection>> GetCollectionDescendants(int collectionId)
        {
            List<Collection> hierarchy = await _unitOfWork.CollectionRepository.GetHierarchyRecursively(collectionId);
            return hierarchy.Where(c => c.Id != collectionId);
        }

        private async Task<Collection> GetCollection(int collectionId)
        {
            return await _unitOfWork.CollectionRepository
                .GetByIdAsync(collectionId) 
                ?? throw new NotFoundException("Collections", collectionId);
        }

        private CustomValidationException HandleMissingPathError(int collectionId)
        {
            _logger.LogError(
                "{message}. CollectionId: {collectionId}",
                MISSING_COLLECTION_PATH_ERROR_MESSAGE,
                collectionId
            );
            return new CustomValidationException(BAD_CONFIGURATION_ERROR_MESSAGE);
        }

        private static string GenerateDescendantNamePath(string path, string oldPath, string newPath)
        {
            string segmentsToKeep = path[oldPath.Length..];
            return $"{newPath}{segmentsToKeep}";
        }

        private static void ValidateDescendantParentCase(IEnumerable<Collection> descendants, int parentId)
        {
            Collection? descendant = descendants.FirstOrDefault(c => c.Id.Equals(parentId));
            if (descendant != null) throw new CustomValidationException(DESCENDANT_PARENT_CASE_ERROR_MESSAGE);
        }
    }
}
