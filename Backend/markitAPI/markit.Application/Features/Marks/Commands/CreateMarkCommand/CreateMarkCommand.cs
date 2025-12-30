using System.Transactions;
using AutoMapper;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.CreateMarkCommand
{
    public class CreateMarkCommand : IRequest<MarkViewModel>
    {
        public string Name { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public int CreatorId { get; set; }
        public int CollectionId { get; set; }
        public List<BlockViewModel> Blocks { get; set; } = new();
    }

    public class CreateMarkCommandHandler : IRequestHandler<CreateMarkCommand, MarkViewModel>
    {
        private readonly IDocumentJobService<MarkDocument> _documentJobService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateMarkCommandHandler(
            IDocumentJobService<MarkDocument> documentJobService,
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _documentJobService = documentJobService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(CreateMarkCommand request, CancellationToken cancellationToken)
        {            
            // Assign main collectionId if empty
            if (request.CollectionId.Equals(0))
            {
                request.CollectionId = (await GetMainCollection(request.CreatorId)).Id;
            }

            await ValidateCreatorExistency(request.CreatorId);
            await ValidateCollection(request.CollectionId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                Mark mark = _mapper.Map<Mark>(request);
                await AddMarkAsync(mark);
                CreateDocumentBackgroundJob(mark, request.CreatorId);
                var markViewModel = _mapper.Map<MarkViewModel>(mark);
            scope.Complete();

            return markViewModel;
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
           _ = await _unitOfWork.CreatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task ValidateCollection(int collectionId)
        {
            _ = await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
        }
        private async Task<Collection> GetMainCollection(int creatorId)
        {
            var mainCollection = await _unitOfWork.CollectionRepository
                .GetAsync(c =>
                    c.IsMain.Equals(true)
                    && c.CreatorId.Equals(creatorId)
                );

            return mainCollection.FirstOrDefault()
                ?? throw new CustomValidationException($"The main collection of the creator with id:{creatorId} is not configured.");
        }

        private async Task AddMarkAsync(Mark mark) => await _unitOfWork.MarkRepository.AddAsync(mark);

        private void CreateDocumentBackgroundJob(Mark mark, int creatorId)
        {
            MarkDocument document = new(mark.Name, mark.Id, creatorId);
            _documentJobService.ScheduleAddAsync(
                document,
                continueWith: () => _unitOfWork.MarkRepository.UpdateSyncModelAsync(mark.Id, document.Id)
            );
        }
    }
}
