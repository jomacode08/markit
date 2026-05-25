using markit.Application.Common.Helpers;
using markit.Application.Common.Helpers.Services;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Collections.Queries.GetCollectionItemsPagedQuery
{
    public class GetCollectionItemsPagedQuery(GetCollectionItemsPagedQueryDto dto, string userId) : IRequest<CollectionItemPage>
    {
        public CollectionItemPageRequest PaginationRequest = new()
        {
            UserId = userId,
            PageSize = dto.PageSize,
            Cursor = dto.Cursor,
            SortOrder = dto.SortOrder,
            Filters = dto.Filters,
        };
    }

    public class GetCollectionItemsPagedQueryHandler : IRequestHandler<GetCollectionItemsPagedQuery, CollectionItemPage>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly CollectionItemService _collectionItemService;
        private readonly UserManager<AppUser> _userManager;

        public GetCollectionItemsPagedQueryHandler(
            IUnitOfWork unitOfWork,
            CollectionItemService collectionItemService,
            UserManager<AppUser> userManager
        )
        {
            _unitOfWork = unitOfWork;
            _collectionItemService = collectionItemService;
            _userManager = userManager;
        }

        public async Task<CollectionItemPage> Handle(GetCollectionItemsPagedQuery request, CancellationToken cancellationToken)
        {
            CollectionItemPageRequest paginationRequest = request.PaginationRequest;
            await ValidateUserExistence(paginationRequest.UserId);

            if (paginationRequest.Filters.CollectionId.HasValue) {
                await  ValidateCollection(paginationRequest.Filters.CollectionId.GetValueOrDefault(), paginationRequest.UserId);
            }

            return await GetItemsAsync(paginationRequest);
        }

        private async Task ValidateCollection(int collectionId, string userId)
        {
            Collection collection = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateUser(userId);
        }

        private async Task ValidateUserExistence(string userId)
        {
            _ = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUser", userId);
        }

        private async Task<CollectionItemPage> GetItemsAsync(CollectionItemPageRequest paginationRequest)
        {
            return await _collectionItemService.GetItemsPageAsync(paginationRequest);
        }
    }
}
