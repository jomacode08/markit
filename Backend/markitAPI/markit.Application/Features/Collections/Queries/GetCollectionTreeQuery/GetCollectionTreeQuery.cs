using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Features.Collections.Queries.GetMainCollectionByCreator;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;
using markit.Application.Exceptions;

namespace markit.Application.Features.Collections.Queries.GetCollectionTreeQuery
{
    public class GetCollectionTreeQuery(int creatorId) : IRequest<TreeNode>
    {
        public int CreatorId { get; init; } = creatorId;
    }

    public class GetCollectionTreeQueryHandler : IRequestHandler<GetCollectionTreeQuery, TreeNode>
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

        public async Task<TreeNode> Handle(GetCollectionTreeQuery request, CancellationToken cancellationToken)
        {
            int mainCollectionId = await GetMainCollectionIdAsync(request.CreatorId);
            List<TreeNode> hierarchy = await GetTreeNodes(mainCollectionId);
            return BuildTree(hierarchy, request.CreatorId);
        }

        private TreeNode BuildTree(List<TreeNode> hierarchy, int creatorId)
        {
            ILookup<string?, TreeNode> lookup = hierarchy.ToLookup(x => x.ParentKey);

            foreach (TreeNode node in hierarchy)
            {
                node.Children = lookup[node.Key];
            }

            TreeNode? root = lookup[null].FirstOrDefault();
            
            if (root is null)
            {
                _logger.LogError("{message}, CreatorId: {creatorId}", MAIN_COLLECTION_BAD_CONFIGURATION_LOG_MESSAGE, creatorId);
                throw new CustomValidationException(BAD_CONFIGURATION_ERROR_MESSAGE);
            }

            return root;
        }

        private async Task<List<TreeNode>> GetTreeNodes(int mainCollectionId)
        {
            List<Collection> hierarchy = await _unitOfWork.CollectionRepository
                .GetHierarchyRecursively(rootCollectionId: mainCollectionId);
            return [.. hierarchy.Select(
                c => new TreeNode
                {
                    Key = c.Id.ToString(),
                    ParentKey = c.ParentId?.ToString(),
                    Data = c.Id.ToString(),
                    Label = c.Name
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
