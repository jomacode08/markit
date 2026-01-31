using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Features.Collections.Commands.CreateCollectionCommand;
using markit.Application.Helpers;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Creators.Commands.CreateCreator
{
    public class CreateCreatorCommand : IRequest<int>
    {
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public DateOnly? BirthDate { get; set; }
        public Gender? Gender { get; set; }
    }

    public class CreateCreatorCommandHandler : IRequestHandler<CreateCreatorCommand, int>
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMediator _mediator;

        public CreateCreatorCommandHandler(IMapper mapper, IUnitOfWork unitOfWork, IMediator mediator)
        {
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _mediator = mediator;
        }

        public async Task<int> Handle(CreateCreatorCommand request, CancellationToken cancellationToken)
        {
            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
            
            Creator creator = _mapper.Map<Creator>(request);
            await AddCreator(creator);
            await AddMainCollection(creator.Id);

            scope.Complete();
            return creator.Id;
        }

        private async Task<Creator> AddCreator(Creator creator)
        {
            return await _unitOfWork.CreatorRepository.AddAsync(creator);
        }

        private async Task AddMainCollection(int creatorId)
        {
            CreateCollectionCommand createCollectionCommand = new()
            {
                Name = GeneralConstant.Marks.MAIN_COLLECTION_NAME,
                IsMain = true,
                CreatorId = creatorId
            };

            await _mediator.Send(createCollectionCommand);
        }
    }
}
