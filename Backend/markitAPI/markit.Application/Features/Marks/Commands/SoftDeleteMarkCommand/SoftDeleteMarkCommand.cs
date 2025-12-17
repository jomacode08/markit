using markit.Application.Common.Exceptions;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Marks.Commands.DeleteMarkCommand
{
    public class SoftDeleteMarkCommand : IRequest<bool>
    {
        public int Id { get; set; }
        public int CreatorId { get; set; }
    }

    public class SoftDeleteMarkCommandHandler : IRequestHandler<SoftDeleteMarkCommand, bool>
    {
        private readonly IDocumentJobService<MarkDocument> _documentJobService;
        private readonly IDocumentRepository<MarkDocument> _documentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SoftDeleteMarkCommandHandler(
            IDocumentJobService<MarkDocument> documentJobService,
            IDocumentRepository<MarkDocument> documentRepository,
            IUnitOfWork unitOfWork
        )
        {
            _documentJobService = documentJobService;
            _documentRepository = documentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SoftDeleteMarkCommand request, CancellationToken cancellationToken)
        {
            Mark mark = await ValidateMarkExistency(request.Id, request.CreatorId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                SoftDeleteMark(mark);
                await SoftDeleteBlocks(mark.Id);
                await _unitOfWork.Complete();
                await CreateDocumentBackgroundJob(mark);
            scope.Complete();

            return true;
        }

        private async Task<Mark> ValidateMarkExistency(int markId, int creatorId)
        {
            Mark mark = await _unitOfWork.markRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);
            mark.ValidateCreator(creatorId);
            return mark;
        }

        private void SoftDeleteMark(Mark mark)
        {
            _unitOfWork.markRepository.SoftDeleteEntity(mark);
        }

        private async Task SoftDeleteBlocks(int markId)
        {
            var blocks = await _unitOfWork.blockRepository.GetAsync(b => b.MarkId.Equals(markId));
            _unitOfWork.blockRepository.SoftDeleteRangeEntity([.. blocks]);
        }

        private async Task CreateDocumentBackgroundJob(Mark mark)
        {
            if (mark.DocumentId == null) return;

            MarkDocument document = await _documentRepository.GetByIdAsync(mark.DocumentId);
            document.Enabled = false;

            _documentJobService.ScheduleUpdateAsync(
                document,
                continueWith: () => _unitOfWork.markRepository.UpdateSyncModelAsync(mark.Id, document.Id)
            );
        }
    }
}
