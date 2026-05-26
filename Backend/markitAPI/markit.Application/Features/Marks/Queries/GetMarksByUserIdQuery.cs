using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace markit.Application.Features.Marks.Queries
{
    public class GetMarksByUserIdQuery : IRequest<List<MarkViewModel>>
    {
        public string UserId { get; set; } = string.Empty;
    }

    public class GetMarksByUserIdHandler : IRequestHandler<GetMarksByUserIdQuery, List<MarkViewModel>>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly UserManager<AppUser> _userManager;

        public GetMarksByUserIdHandler(IUnitOfWork unitOfWork, IMapper mapper, UserManager<AppUser> userManager)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _userManager = userManager;
        }

        public async Task<List<MarkViewModel>> Handle(GetMarksByUserIdQuery request, CancellationToken cancellationToken)
        {
            await ValidateUserExistence(request.UserId);
            // Get the marks by the user
            var marks = await _unitOfWork.MarkRepository
                .GetAsync(m => m.Collection != null && m.Collection.UserId.Equals(request.UserId));

            return _mapper.Map<List<MarkViewModel>>(marks);
        }

        private async Task ValidateUserExistence(string userId)
        {
            _ = await _userManager.FindByIdAsync(userId)
                ?? throw new NotFoundException("AppUser", userId);
        }
    }
}
