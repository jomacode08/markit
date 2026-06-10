using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System.Transactions;

namespace markit.Application.Features.Notebooks.Commands.CreateNotebookCommand
{
    public class CreateNotebookCommand : IRequest<NotebookViewModel>
    {
        public string Name { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public int CollectionId { get; set; }
        public List<BlockViewModel> Blocks { get; set; } = [];
    }

    public class CreateNotebookCommandHandler : IRequestHandler<CreateNotebookCommand, NotebookViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public CreateNotebookCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper,
            UserManager<AppUser> userManager
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<NotebookViewModel> Handle(CreateNotebookCommand request, CancellationToken cancellationToken)
        {            
            // Assign main collectionId if empty
            if (request.CollectionId.Equals(0))
            {
                request.CollectionId = (await GetMainCollection(request.UserId)).Id;
            }

            await ValidateUserExistence(request.UserId);
            await ValidateCollection(request.CollectionId, request.UserId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                Notebook notebook = _mapper.Map<Notebook>(request);
                await AddNotebooksAsync(notebook);
                var notebookViewModel = _mapper.Map<NotebookViewModel>(notebook);
            scope.Complete();

            return notebookViewModel;
        }

        private async Task ValidateUserExistence(string userId)
        {
            _ = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUser", userId);
        }

        private async Task ValidateCollection(int collectionId, string userId)
        {
            Collection collection = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
            collection.ValidateUser(userId);
        }
        private async Task<Collection> GetMainCollection(string userId)
        {
            var mainCollection = await _unitOfWork.CollectionRepository
                .GetAsync(c =>
                    c.IsMain.Equals(true)
                    && c.UserId.Equals(userId)
                );

            return mainCollection.FirstOrDefault()
                ?? throw new CustomValidationException($"The main collection of the user with id:{userId} is not configured.");
        }

        private async Task AddNotebooksAsync(Notebook notebook) => await _unitOfWork.NotebookRepository.AddAsync(notebook);
    }
}
