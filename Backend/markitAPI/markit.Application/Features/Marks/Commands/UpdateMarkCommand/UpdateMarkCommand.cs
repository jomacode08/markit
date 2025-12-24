using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.MeiliSearch;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Application.Models.MeiliSearch.Documents;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Marks.Commands.UpdateMarkCommand
{
    public class UpdateMarkCommand(int id, UpdateMarkDto dto) : IRequest<MarkViewModel>
    {
        public int Id { get; set; } = id;
        public string InputName { get; set; } = dto.InputName;
        public string? Emoji { get; set; } = dto.Emoji;
        public List<BlockViewModel> Blocks { get; set; } = dto.Blocks;
        public int CreatorId { get; set; } = dto.CreatorId;
    }

    public class UpdateMarkCommandHandler : IRequestHandler<UpdateMarkCommand, MarkViewModel>
    {
        private readonly IDocumentRepository<MarkDocument> _documentRepository;
        private readonly IDocumentJobService<MarkDocument> _documentJobService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateMarkCommandHandler(
            IDocumentRepository<MarkDocument> documentRepository,
            IDocumentJobService<MarkDocument> documentJobService,
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _documentRepository = documentRepository;
            _documentJobService = documentJobService;
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(UpdateMarkCommand request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistency(request.CreatorId);
            Mark mark = await ValidateMarkExistency(request.Id, request.CreatorId);
            bool nameChanged = request.InputName != mark.Name;

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                _mapper.Map(request, mark, typeof(UpdateMarkCommand), typeof(Mark));
                await UpdateMarkAsync(mark);
                if (nameChanged) await CreateDocumentBackgroundJob(mark);
                var markVm = _mapper.Map<MarkViewModel>(mark);
            scope.Complete();

            return markVm;
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
            _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task<Mark> ValidateMarkExistency(int markId, int creatorId)
        {
            Mark? mark = await _unitOfWork.markRepository.GetByIdAsync(markId, "Blocks,Collection")
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
