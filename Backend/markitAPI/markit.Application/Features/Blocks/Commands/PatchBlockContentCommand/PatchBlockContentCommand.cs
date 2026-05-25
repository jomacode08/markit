using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Commands.PatchBlockContentCommand;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Blocks.Commands.PatchBlockCommand
{
    public class PatchBlockContentCommand(int id, PatchBlockDto dto) : IRequest<BlockViewModel>
    {
        public int Id { get; set; } = id;
        public string Content { get; set; } = dto.Content;
        public string UserId { get; set; } = dto.UserId;
    }

    public class PatchBlockCommandHandler : IRequestHandler<PatchBlockContentCommand, BlockViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public PatchBlockCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<BlockViewModel> Handle(PatchBlockContentCommand request, CancellationToken cancellationToken)
        {
            Block block = await ValidateBlock(request.Id, request.UserId);

            block.Content = request.Content;
            await _unitOfWork.BlockRepository.UpdateAsync(block);

            return _mapper.Map<BlockViewModel>(block);
        }

        private async Task<Block> ValidateBlock(int blockId, string userId)
        {
            Block block = await _unitOfWork.BlockRepository.GetByIdAsync(blockId, "Mark")
                ?? throw new NotFoundException("Block", blockId);
            block.Mark?.ValidateUser(userId);
            return block;
        }
    }
}
