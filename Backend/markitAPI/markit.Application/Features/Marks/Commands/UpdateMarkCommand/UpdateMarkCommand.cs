using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
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
        public string UserId { get; set; } = dto.UserId;
    }

    public class UpdateMarkCommandHandler : IRequestHandler<UpdateMarkCommand, MarkViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateMarkCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(UpdateMarkCommand request, CancellationToken cancellationToken)
        {
            Notebook mark = await ValidateMark(request.Id, request.UserId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                _mapper.Map(request, mark, typeof(UpdateMarkCommand), typeof(Notebook));
                await UpdateMarkAsync(mark);
                var markVm = _mapper.Map<MarkViewModel>(mark);
            scope.Complete();

            return markVm;
        }

        private async Task<Notebook> ValidateMark(int markId, string userId)
        {
            Notebook? mark = await _unitOfWork.MarkRepository.GetByIdAsync(markId, "Blocks,Collection")
                ?? throw new NotFoundException("Mark", markId);
            mark.ValidateUser(userId);
            return mark;
        }

        private async Task UpdateMarkAsync(Notebook mark) => await _unitOfWork.MarkRepository.UpdateAsync(mark);
    }
}
