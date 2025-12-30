using AutoMapper;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Creators.Queries;
using markit.Application.Features.Creators.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Creators.Commands.UpdateCreator
{
    public class UpdateCreatorCommand(int id, UpdateCreatorDto dto) : IRequest<CreatorViewModel>
    {

        public int Id { get; set; } = id;
        public string FirstName { get; set; } = dto.FirstName;
        public string LastName { get; set; } = dto.LastName;
        public string BirthDate { get; set; } = dto.BirthDate;
        public Gender Gender { get; set; } = dto.Gender;
    }

    public class UpdateCreatorCommandHandler : IRequestHandler<UpdateCreatorCommand, CreatorViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;
        private readonly IAppUserService _appUserService;

        public UpdateCreatorCommandHandler(IUnitOfWork unitOfWork, IMapper mapper, IAppUserService appUserService, IMediator mediator)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _appUserService = appUserService;
            _mediator = mediator;
        }

        public async Task<CreatorViewModel> Handle(UpdateCreatorCommand request, CancellationToken cancellationToken)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);

            await UpdateCreator(request);
            await UpdateAppUser(request);

            var creatorUpdated = await _mediator.Send(
                new GetCreatorByIdQuery(request.Id)
            );

            scope.Complete();
            return creatorUpdated;
        }

        private async Task UpdateCreator(UpdateCreatorCommand request) {
            // Get creator from database
            Creator creator = await _unitOfWork.CreatorRepository.GetByIdAsync(request.Id)
            ?? throw new NotFoundException("Creator", request.Id);

            // Mapping the request values to creator
            _mapper.Map(request, creator, typeof(UpdateCreatorCommand), typeof(Creator));

            // Update creator
            await _unitOfWork.CreatorRepository.UpdateAsync(creator);
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
