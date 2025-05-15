using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Blocks.Commands.PatchBlockCommand
{
    public class PatchBlockContentCommand : IRequest<BlockViewModel>
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public int CreatorId { get; set; }
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
            var block = await _unitOfWork.blockRepository.GetByIdAsync(blockId, "Mark")
                ?? throw new NotFoundException("Block", blockId);

            if (block.Mark?.CollectionId != creatorId) throw new UnauthorizedAccessException();

            return block;
        }
    }
}
