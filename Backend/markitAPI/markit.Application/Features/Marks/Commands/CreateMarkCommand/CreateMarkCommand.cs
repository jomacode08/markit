using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Marks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.CreateMarkCommand
{
    public class CreateMarkCommand : IRequest<MarkViewModel>
    {
        public string Name { get; set; } = string.Empty;
        public int CreatorId { get; set; }
        public int CollectionId { get; set; }
        public List<BlockViewModel> Blocks { get; set; } = new();
    }

    public class CreateMarkCommandHandler : IRequestHandler<CreateMarkCommand, MarkViewModel>
    {
        private IUnitOfWork _unitOfWork;
        private IMapper _mapper;

        public CreateMarkCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<MarkViewModel> Handle(CreateMarkCommand request, CancellationToken cancellationToken)
        {
            await ValidateCreatorExistency(request.CreatorId);
            
            // Assign main collectionId if empty
            if (request.CollectionId.Equals(0))
            {
                request.CollectionId = (await GetMainCollection(request.CreatorId)).Id;
            }

            await ValidateCollection(request.CollectionId);

            // Create the mark
            Mark mark = _mapper.Map<Mark>(request);
            _unitOfWork.markRepository.AddEntity(mark);

            // Complete the transaction
            await _unitOfWork.Complete();
            return _mapper.Map<MarkViewModel>(mark);
        }

        private async Task ValidateCreatorExistency(int creatorId)
        {
           _ = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
                ?? throw new NotFoundException("Creator", creatorId);
        }

        private async Task ValidateCollection(int collectionId)
        {
            _ = await _unitOfWork.collectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collection", collectionId);
        }

        private async Task<Collection> GetMainCollection(int creatorId)
        {
            var mainCollection = await _unitOfWork.collectionRepository
                .GetAsync(c =>
                    c.IsMain.Equals(true)
                    && c.CreatorId.Equals(creatorId)
                );

            return mainCollection.FirstOrDefault()
                ?? throw new CustomValidationException($"The main collection of the creator with id:{creatorId} is not configured.");
        }
    }
}
