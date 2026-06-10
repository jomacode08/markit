using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Notebooks.Commands.DeleteNotebookCommand
{
    public class SoftDeleteNotebookCommand(int id, string userId) : IRequest<bool>
    {
        public int Id { get; set; } = id;
        public string UserId { get; set; } = userId;
    }

    public class SoftDeleteNotebookCommandHandler : IRequestHandler<SoftDeleteNotebookCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public SoftDeleteNotebookCommandHandler(
            IUnitOfWork unitOfWork
        )
        {
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> Handle(SoftDeleteNotebookCommand request, CancellationToken cancellationToken)
        {
            Notebook notebook = await ValidateNotebookExistence(request.Id, request.UserId);
            SoftDeleteNotebook(notebook);
            await SoftDeleteBlocks(notebook.Id);
            await _unitOfWork.Complete();
            return true;
        }

        private async Task<Notebook> ValidateNotebookExistence(int notebookId, string userId)
        {
            Notebook notebook = await _unitOfWork.NotebookRepository.GetByIdAsync(notebookId, "Collection")
                ?? throw new NotFoundException("Notebook", notebookId);
            notebook.ValidateUser(userId);
            return notebook;
        }

        private void SoftDeleteNotebook(Notebook notebook)
        {
            _unitOfWork.NotebookRepository.SoftDeleteEntity(notebook);
        }

        private async Task SoftDeleteBlocks(int notebookId)
        {
            var blocks = await _unitOfWork.BlockRepository.GetAsync(b => b.NotebookId.Equals(notebookId));
            _unitOfWork.BlockRepository.SoftDeleteRangeEntity([.. blocks]);
        }
    }
}
