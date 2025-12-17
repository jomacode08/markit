using AutoMapper;
using markit.Application.Common.Exceptions;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Marks.Commands.RenameMarkCommand
{
    public class RenameMarkCommand : IRequest<MarkViewModel>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Emoji { get; set; }
        public int CreatorId { get; set; }
    }

    public class RenameMarkCommandHandler : IRequestHandler<RenameMarkCommand, MarkViewModel>
    {
        private readonly IDocumentJobService<MarkDocument> _documentJobService;
        private readonly IDocumentRepository<MarkDocument> _documentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RenameMarkCommandHandler(
            IDocumentJobService<MarkDocument> documentJobService,
            IDocumentRepository<MarkDocument> documentRepository,
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _documentJobService = documentJobService;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(RenameMarkCommand request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistency(request.CreatorId);
            var mark = await ValidateMarkExistency(request.Id, request.CreatorId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                mark.Name = request.Name;
                mark.Emoji = request.Emoji;
                await UpdateMarkAsync(mark);
                await CreateDocumentBackgroundJob(mark);
                var markViewModel = _mapper.Map<MarkViewModel>(mark);
            scope.Complete();

            return markViewModel;
        }

        private async Task ValidateCreatorExistency(int creatorId) {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<Mark> ValidateMarkExistency(int markId, int creatorId)
        {
            Mark? mark = await _unitOfWork.markRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);
            mark.ValidateCreator(creatorId);
            return mark;
        }

        private async Task UpdateMarkAsync(Mark mark) => await _unitOfWork.markRepository.UpdateAsync(mark);

        private async Task CreateDocumentBackgroundJob(Mark mark)
        {
            if (mark.DocumentId == null) return;

            MarkDocument document = await _documentRepository.GetByIdAsync(mark.DocumentId);
            document.Name = mark.Name;

            _documentJobService.ScheduleUpdateAsync(
                document,
                continueWith: () => _unitOfWork.markRepository.UpdateSyncModelAsync(mark.Id, document.Id)
            );
        }
    }
}
