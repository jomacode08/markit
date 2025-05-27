using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsForGridQuery
{
    public class GetCollectionByIdQuery : IRequest<CollectionViewModel>
    {
        public int CollectionId { get; set; }
        public int CreatorId { get; set; }
        public bool IncludeCollectionITems { get; set; }
    }

    public class GetCollectionItemsQueryHandler : IRequestHandler<GetCollectionByIdQuery, CollectionViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetCollectionByIdQuery> _logger;

        public GetCollectionItemsQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, ILogger<GetCollectionByIdQuery> logger)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CollectionViewModel> Handle(GetCollectionByIdQuery request, CancellationToken cancellationToken)
        {
            // Validations
            await ValidateCreatorExistency(request.CreatorId);
            var collection = await ValidateCollection(request.CollectionId, request.CreatorId);

            return await MapCollection(collection, request.IncludeCollectionITems);
        }

        private async Task<Collection> ValidateCollection(int collectionId, int creatorId)
        {
            var collection = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);

            if (collection.CreatorId != creatorId) throw new UnauthorizedAccessException();

            return collection;
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<List<CollectionItem>> MapCollectionItems(int collectionId)
        {
            // Get child collections and marks
            var collections = await _unitOfWork.collectionRepository.GetAsync(c => c.ParentId.Equals(collectionId), null, "Marks");
            var marks = await _unitOfWork.markRepository.GetAsync(m => m.CollectionId == collectionId, null, "Blocks");

            // Map to collectionItem
            var collectionItems = _mapper.Map<List<CollectionItem>>(collections);
            var markItems = _mapper.Map<List<CollectionItem>>(marks);

            return [.. collectionItems, .. markItems];
        }

        private async Task<CollectionViewModel> MapCollection(Collection collection, bool includeItems)
        {
            // Mapping Collection to CollectionViewModel
            CollectionViewModel collectionVm = _mapper.Map<CollectionViewModel>(collection);

            // Add path
            try
            {
                collectionVm.Path = CreateCollectionPath(collection);
            }
            catch (FormatException ex) {
                _logger.LogError(ex.Message, ex);
            }

            // Add CollectionItems if it's necesary
            if (includeItems)
            {
                collectionVm.CollectionItems = await MapCollectionItems(collection.Id);
            }

            return collectionVm;
        }

        private static List<CollectionPath> CreateCollectionPath(Collection collection)
        {
            var path = new List<CollectionPath>();
            const string PATH_SPLITER = "/";
            string[] pathIds = collection.Path?.Split(PATH_SPLITER, StringSplitOptions.RemoveEmptyEntries) ?? [];
            string[] pathNames = collection.PathNames.Split(PATH_SPLITER, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < pathIds.Length; i++)
            {
                if (!int.TryParse(pathIds[i], out int collectionId))
                    throw new FormatException($"The path of the collection with id: { collection.Id } doesn't have the correct format.");

                path.Add(new CollectionPath
                {
                    CollectionId = collectionId,
                    Name = pathNames[i]
                });
            }

            return path;
        }
    }
}
