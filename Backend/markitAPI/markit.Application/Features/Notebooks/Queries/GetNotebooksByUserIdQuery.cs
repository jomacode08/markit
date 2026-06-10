using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Notebooks.Queries
{
    public class GetNotebooksByUserIdQuery : IRequest<List<NotebookViewModel>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetNotebooksByUserIdHandler : IRequestHandler<GetNotebooksByUserIdQuery, List<NotebookViewModel>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public GetNotebooksByUserIdHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<List<NotebookViewModel>> Handle(GetNotebooksByUserIdQuery request, CancellationToken cancellationToken)
        {
            await ValidateUserExistence(request.UserId);
            // Get the notebooks by the user
            var notebooks = await _unitOfWork.NotebookRepository
                .GetAsync(m => m.Collection != null && m.Collection.UserId.Equals(request.UserId));

            return _mapper.Map<List<NotebookViewModel>>(notebooks);
        }

        private async Task ValidateUserExistence(string userId)
        {
            _ = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUser", userId);
        }
    }
}
