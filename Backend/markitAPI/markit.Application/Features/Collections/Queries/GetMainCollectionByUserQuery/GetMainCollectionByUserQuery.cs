using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using Microsoft.Extensions.Logging;

namespace markit.Application.Features.Collections.Queries.GetMainCollectionByUser
{
    public class GetMainCollectionByUserQuery(string userId) : IRequest<CollectionViewModel>
    {
       public string UserId { get; set; } = userId;
    }

    public class GetMainCollectionByUserQueryHandler : IRequestHandler<GetMainCollectionByUserQuery, CollectionViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly ILogger<GetMainCollectionByUserQuery> _logger;

        public GetMainCollectionByUserQueryHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            ILogger<GetMainCollectionByUserQuery> logger
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<CollectionViewModel> Handle(GetMainCollectionByUserQuery request, CancellationToken cancellationToken)
        {
            Collection mainCollection = await GetMainCollection(request.UserId);
            return MapCollection(mainCollection);
        }

        private async Task<Collection> GetMainCollection(string userId)
        {
            var result = await _unitOfWork.CollectionRepository.GetAsync(c => c.UserId == userId && c.IsMain)
                ?? throw new CustomValidationException($"The main collection of the user with ID: {userId} must be configured");

            return result[0];
        }

        private CollectionViewModel MapCollection(Collection collection)
        {
            CollectionViewModel collectionVm = _mapper.Map<CollectionViewModel>(collection);

            // Add path
            try
            {
                collectionVm.Path = collection.CreatePath();
            }
            catch (FormatException ex)
            {
                _logger.LogError("{message}", ex.Message);
            }

            return collectionVm;
        }
    }
}
