using AutoMapper;
using markit.Application.Contracts.Authentication;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Creators.Queries.ViewModels;
using markit.Application.Models.Authentication.AppUser;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Creators.Queries
{
    public class GetCreatorByIdQuery(int id) : IRequest<CreatorViewModel>
    {
        public int Id { get; set; } = id;
    }

    public class GetCreatorByIdQueryHandler : IRequestHandler<GetCreatorByIdQuery, CreatorViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IAppUserService _appUserService;

        public GetCreatorByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper, IAppUserService appUserService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _appUserService = appUserService;
        }

        public async Task<CreatorViewModel> Handle(GetCreatorByIdQuery request, CancellationToken cancellationToken)
        {
            Creator creator = await _unitOfWork.CreatorRepository.GetByIdAsync(request.Id) 
            ?? throw new NotFoundException("Creator", request.Id);

            AppUser user = _appUserService.GetUserByCreatorIdAsync(creator.Id);
            
            CreatorViewModel creatorVm = _mapper.Map<CreatorViewModel>(creator);

            // Access system properties
            creatorVm.Email = user.Email!;
            creatorVm.Picture = user.Picture;
            creatorVm.RegistrationConfirmed = user.RegistrationConfirmed;

            return creatorVm;
        }
    }
}
