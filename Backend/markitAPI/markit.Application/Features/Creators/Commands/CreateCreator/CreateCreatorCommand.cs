using AutoMapper;
using markit.Application.Contracts.Autentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Models.Authentication;
using markit.Application.Models.Authentication.Enums;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Creators.Commands.CreateCreator
{
    public class CreateCreatorCommand : IRequest<Unit>
    {
        public string Email { get; set; } = string.Empty;
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public AccessType AccessType { get; set; }
        public string? Password { get; set; }
        public string? Picture { get; set; }
        public DateOnly? BirthDate { get; set; }
        public Gender? Gender { get; set; }
    }

    public class CreateCreatorCommandHandler : IRequestHandler<CreateCreatorCommand, Unit>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAuthService _authService;
        public CreateCreatorCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IAuthService authService)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _authService = authService;
        }

        public async Task<Unit> Handle(CreateCreatorCommand request, CancellationToken cancellationToken)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            
            Creator creator = _mapper.Map<Creator>(request);
            await AddCreator(creator);
            await AddSystemAccess(request, creator.Id);

            scope.Complete();
            return Unit.Value;
        }

        private async Task<Creator> AddCreator(Creator creator)
        {
            return await _unitOfWork.creatorRepository.AddAsync(creator);
        }

        private async Task AddSystemAccess(CreateCreatorCommand request, int creatorId)
        {
            UserViewModel user = new(
                request.Email,
                request.Password,
                request.FirstName,
                request.LastName,
                request.Picture,
                request.AccessType
            );

            await _authService.CreateIdentityUser(user, creatorId);
        }
    }
}
