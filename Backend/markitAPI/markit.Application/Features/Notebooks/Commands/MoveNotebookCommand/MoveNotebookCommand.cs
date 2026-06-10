using markit.Application.Common.Exceptions;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Notebooks.Commands.MoveNotebookCommand
{
    public class MoveNotebookCommand(int notebookId, int collectionId, string userId) : IRequest<Unit>
    {
        public int NotebookId { get; init; } = notebookId;
        public int CollectionId { get; init; } = collectionId;
        public string UserId { get; init; } = userId;
    }

    public class MoveNotebookCommandHandler : IRequestHandler<MoveNotebookCommand, Unit>
    {
        private readonly IUnitOfWork _unitOfWork;

        public MoveNotebookCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<Unit> Handle(MoveNotebookCommand request, CancellationToken cancellationToken)
        {
            Notebook notebook = await GetNotebookAsync(request.NotebookId);
            Collection sourceCollection = await GetCollectionAsync(notebook.CollectionId);
            Collection destinyCollection = await GetCollectionAsync(request.CollectionId);
            
            if (sourceCollection.UserId != request.UserId) throw new ForbiddenResourceException("Notebooks", notebook.Id, request.UserId);
            if (destinyCollection.UserId != request.UserId) throw new ForbiddenResourceException("Collections", destinyCollection.Id, request.UserId);
            if (notebook.CollectionId.Equals(request.CollectionId)) return Unit.Value;

            await MoveNotebookAsync(notebook, newCollectionId: destinyCollection.Id);
            return Unit.Value;
        }

        private async Task MoveNotebookAsync(Notebook notebook, int newCollectionId)
        {
            notebook.CollectionId = newCollectionId;
            await _unitOfWork.NotebookRepository.UpdateAsync(notebook);
        }

        private async Task<Collection> GetCollectionAsync(int collectionId)
        {
            return await _unitOfWork.CollectionRepository.GetByIdAsync(collectionId)
                ?? throw new NotFoundException("Collections", collectionId);
        }

        private async Task<Notebook> GetNotebookAsync(int notebookId)
        {
            return await _unitOfWork.NotebookRepository.GetByIdAsync(notebookId)
                ?? throw new NotFoundException("Notebooks", notebookId);
        }
    }
}
