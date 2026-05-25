using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Marks.Commands.RenameMarkCommand
{
    public class RenameMarkCommand(int id, RenameMarkDto dto) : IRequest<MarkViewModel>
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = dto.Name;
        public string? Emoji { get; set; } = dto.Emoji;
        public string UserId { get; set; } = dto.UserId;
    }

    public class RenameMarkCommandHandler : IRequestHandler<RenameMarkCommand, MarkViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RenameMarkCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(RenameMarkCommand request, CancellationToken cancellationToken)
        {
            var mark = await ValidateMark(request.Id, request.UserId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                mark.Name = request.Name;
                mark.Emoji = request.Emoji;
                await UpdateMarkAsync(mark);
                var markViewModel = _mapper.Map<MarkViewModel>(mark);
            scope.Complete();

            return markViewModel;
        }

        private async Task<Mark> ValidateMark(int markId, string userId)
        {
            Mark? mark = await _unitOfWork.MarkRepository.GetByIdAsync(markId, "Collection")
                ?? throw new NotFoundException("Mark", markId);
            mark.ValidateUser(userId);
            return mark;
        }

        private async Task UpdateMarkAsync(Mark mark) => await _unitOfWork.MarkRepository.UpdateAsync(mark);
    }
}
