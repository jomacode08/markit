using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Notebooks.Commands.RenameNotebookCommand
{
    public class RenameNotebookCommand(int id, RenameNotebookDto dto) : IRequest<NotebookViewModel>
    {
        public int Id { get; set; } = id;
        public string Name { get; set; } = dto.Name;
        public string? Emoji { get; set; } = dto.Emoji;
        public string UserId { get; set; } = dto.UserId;
    }

    public class RenameNotebookCommandHandler : IRequestHandler<RenameNotebookCommand, NotebookViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public RenameNotebookCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NotebookViewModel> Handle(RenameNotebookCommand request, CancellationToken cancellationToken)
        {
            var notebook = await ValidateNotebook(request.Id, request.UserId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                notebook.Name = request.Name;
                notebook.Emoji = request.Emoji;
                await UpdateNotebookAsync(notebook);
                var notebookViewModel = _mapper.Map<NotebookViewModel>(notebook);
            scope.Complete();

            return notebookViewModel;
        }

        private async Task<Notebook> ValidateNotebook(int notebookId, string userId)
        {
            Notebook? notebook = await _unitOfWork.NotebookRepository.GetByIdAsync(notebookId, "Collection")
                ?? throw new NotFoundException("Notebook", notebookId);
            notebook.ValidateUser(userId);
            return notebook;
        }

        private async Task UpdateNotebookAsync(Notebook notebook) => await _unitOfWork.NotebookRepository.UpdateAsync(notebook);
    }
}
