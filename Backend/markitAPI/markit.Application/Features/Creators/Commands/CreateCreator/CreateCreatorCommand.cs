using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;
using static markit.Application.Helpers.GeneralConstant.Marks;

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
            Collection main = ConstructMainCollection();
            await AddCreator(creator, main);
            await UpdateCollectionPath(main);

            scope.Complete();
            return creator.Id;
        }

        private async Task UpdateCollectionPath(Collection main)
        {
            main.Path = $"/{main.Id}";
            await _unitOfWork.CollectionRepository.UpdateAsync(main);
        }

        private async Task<Creator> AddCreator(Creator creator, Collection mainCollection)
        {
            creator.Collections = [mainCollection];
            return await _unitOfWork.CreatorRepository.AddAsync(creator);
        }

        private static Collection ConstructMainCollection() => new()
        {
            Name = MAIN_COLLECTION_NAME,
            PathNames = $"/{MAIN_COLLECTION_NAME}",
            IsMain = true,
        };
    }
}
