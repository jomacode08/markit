using System.Transactions;
using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Collections.Commands.CreateCollectionCommand
{
    public class CreateCollectionCommand : IRequest<CollectionViewModel>
    {
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public string? Emoji { get; set; }
        public int? ParentId { get; set; }

        public void Deconstruct( out string name,out string userId, out int? parentId )
        {
            name = Name;
            parentId = ParentId;
            userId = UserId;
        }
    }

    public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, CollectionViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public CreateCollectionCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<CollectionViewModel> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
        {
            // Validations
            await ValidateUserExistence(request.UserId);
            await ValidateNameDuplicates(request);
            if (request.IsMain) await ValidateMainCollectionDuplicate(request.UserId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

                var collection = await CreateCollection(request);
                Collection? parent = collection.ParentId.HasValue ? await GetCollection((int)collection.ParentId) : null;
                collection = await UpdateCollectionPath(collection, parent);
                var collectionViewModel = _mapper.Map<CollectionViewModel>(collection);

            scope.Complete();
            return collectionViewModel;
        }

        private async Task ValidateUserExistence(string userId)
        {
            _ = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUser", userId);
        }

        private async Task<Unit> ValidateMainCollectionDuplicate(string userId)
        {
            var mainCollection = await _unitOfWork.CollectionRepository
                .GetAsync(c => c.UserId.Equals(userId) && c.IsMain.Equals(true));

            if (mainCollection.Any()) throw new CustomValidationException("A main collection is already configured for the user");

            return Unit.Value;
        }

        private async Task<Unit> ValidateNameDuplicates(CreateCollectionCommand request)
        {
            (string name, string userId, int? parentId) = request;

            var duplicates = await _unitOfWork.CollectionRepository
                .GetAsync(c =>
                    c.ParentId.Equals(parentId)
                    && c.Name.Equals(name)
                    && c.UserId.Equals(userId)
                );

            if (duplicates.Any()) throw new CustomValidationException(@$"There's already a collection with the name: { name }");

            return Unit.Value;
        }
        private async Task<Collection> GetCollection(int collectionId)
        {
            return await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
            ?? throw new NotFoundException("Collection", collectionId);
        }

        private async Task<Collection> CreateCollection(CreateCollectionCommand request)
        {
            var collection = _mapper.Map<Collection>(request);
            await _unitOfWork.CollectionRepository.AddAsync(collection);
            return collection;
        }

        private async Task<Collection> UpdateCollectionPath(Collection collection, Collection? parent)
        {
            collection.Path = CreatePathIds(collection.Id, parent);
            collection.PathNames = CreatePathNames(collection.Name, parent);
            return await _unitOfWork.CollectionRepository.UpdateAsync(collection);
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
