using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Notebooks.Commands.SetNotebookFavoriteStatusCommand
{
    public class SetNotebookFavoriteStatusCommand(int id, string userId, bool isFavorite) : IRequest<bool>
    {
        public int Id { get; set; } = id;
        public string UserId { get; set; } = userId;
        public bool IsFavorite { get; set; } = isFavorite;
    }

    public class SetNotebookFavoriteStatusCommandHandler : IRequestHandler<SetNotebookFavoriteStatusCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SetNotebookFavoriteStatusCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SetNotebookFavoriteStatusCommand request, CancellationToken cancellationToken)
        {
            Notebook notebook = await ValidateNotebook(request.Id, request.UserId);
            
            notebook.IsFavorite = request.IsFavorite;
            _unitOfWork.NotebookRepository.UpdateEntity(notebook);
         
            await _unitOfWork.Complete();
            return notebook.IsFavorite;
        }

        private async Task<Notebook> ValidateNotebook(int notebookId, string userId)
        {
            Notebook notebook = await _unitOfWork.NotebookRepository.GetByIdAsync(notebookId, "Collection")
                ?? throw new NotFoundException("Notebook", notebookId);
            notebook.ValidateUser(userId);
            return notebook;
        }
    }
}
