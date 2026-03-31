using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Collections.Queries.GetMainCollectionByCreator;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using markit.Application.Exceptions;

namespace markit.Application.Features.Collections.Queries.GetCollectionTreeQuery
{
    public class GetCollectionTreeQuery(int creatorId) : IRequest<CollectionNode>
    {
        public int CreatorId { get; init; } = creatorId;
    }

    public class GetCollectionTreeQueryHandler : IRequestHandler<GetCollectionTreeQuery, CollectionNode>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;
        private readonly ILogger<GetCollectionTreeQueryHandler> _logger;

        private const string MAIN_COLLECTION_BAD_CONFIGURATION_LOG_MESSAGE = "There is a configuration error in the main collection structure.";
        private const string BAD_CONFIGURATION_ERROR_MESSAGE = "The operation can't proceed due to a configuration error.";
        public GetCollectionTreeQueryHandler(
            IUnitOfWork unitOfWork,
            IMediator mediator,
            ILogger<GetCollectionTreeQueryHandler> logger
        )
        {
            _unitOfWork = unitOfWork;
            _mediator = mediator;
            _logger = logger;
        }

        public async Task<CollectionNode> Handle(GetCollectionTreeQuery request, CancellationToken cancellationToken)
        {
            int mainCollectionId = await GetMainCollectionIdAsync(request.CreatorId);
            List<CollectionNode> hierarchy = await GetCollectionHierarchyNodesAsync(mainCollectionId);
            return BuildTree(hierarchy, request.CreatorId);
        }

        private CollectionNode BuildTree(List<CollectionNode> hierarchy, int creatorId)
        {
            ILookup<int?, CollectionNode> lookup = hierarchy.ToLookup(x => x.ParentId);

            foreach (CollectionNode node in hierarchy)
            {
                node.Children = lookup[node.Id];
            }

            CollectionNode? root = lookup[null].FirstOrDefault();
            
            if (root is null)
            {
                _logger.LogError("{message}, CreatorId: {creatorId}", MAIN_COLLECTION_BAD_CONFIGURATION_LOG_MESSAGE, creatorId);
                throw new CustomValidationException(BAD_CONFIGURATION_ERROR_MESSAGE);
            }

            return root;
        }

        private async Task<List<CollectionNode>> GetCollectionHierarchyNodesAsync(int mainCollectionId)
        {
            List<Collection> hierarchy = await _unitOfWork.CollectionRepository
                .GetHierarchyRecursively(rootCollectionId: mainCollectionId);
            return [.. hierarchy.Select(
                n => new CollectionNode
                {
                    Id = n.Id,
                    Name = n.Name,
                    ParentId = n.ParentId,
                    IsMain = n.IsMain,
                    Emoji = n.Emoji,
                })
            ];
        }

        private async Task<int> GetMainCollectionIdAsync(int creatorId)
        {
            GetMainCollectionByCreatorQuery query = new(creatorId);
            return (await _mediator.Send(query)).Id;
        }
    }
}
