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
        public int CreatorId { get; set; } = dto.CreatorId;
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
            Block block = await ValidateBlock(request.Id, request.CreatorId);

            block.Content = request.Content;
            await _unitOfWork.blockRepository.UpdateAsync(block);

            return _mapper.Map<BlockViewModel>(block);
        }

        private async Task<Block> ValidateBlock(int blockId, int creatorId)
        {
            var block = await _unitOfWork.blockRepository.GetByIdAsync(blockId)
                ?? throw new NotFoundException("Block", blockId);
            var mark = await _unitOfWork.markRepository.GetByIdAsync(block.MarkId, "Collection")
                ?? throw new NotFoundException("Mark", block.MarkId);
            mark.ValidateCreator(creatorId);
            return block;
        }
    }
}
