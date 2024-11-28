using AutoMapper;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Creators.Commands.UpdateCreator
{
    public class UpdateCreatorCommand : IRequest<Unit>
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string BirthDate { get; set; } = string.Empty;
        public Gender Gender { get; set; }
    }

    public class UpdateCreatorCommandHandler : IRequestHandler<UpdateCreatorCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAppUserService _appUserService;

        public UpdateCreatorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IAppUserService appUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _appUserService = appUserService;
        }

        public async Task<Unit> Handle(UpdateCreatorCommand request, CancellationToken cancellationToken)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            await UpdateCreator(request);
            await UpdateAppUser(request);

            scope.Complete();
            return Unit.Value;
        }

        private async Task UpdateCreator(UpdateCreatorCommand request) {
            // Get creator from database
            Creator creator = await _unitOfWork.creatorRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("Creator", request.Id);

            // Mapping the request values to creator
            _mapper.Map(request, creator, typeof(UpdateCreatorCommand), typeof(Creator));

            // Update creator
            await _unitOfWork.creatorRepository.UpdateAsync(creator);
        }

        private async Task UpdateAppUser(UpdateCreatorCommand request)
        {
            await _appUserService.UpdateIdentityUser(
                new UpdateAppUserRequest(
                    request.FirstName,
                    request.LastName,
                    true),
                request.Id
            );
        }
    }
}
