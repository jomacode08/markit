using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Blocks.Queries.ViewModels;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;
using System.Transactions;

namespace markit.Application.Features.Notebooks.Commands.UpdateNotebookCommand
{
    public class UpdateNotebookCommand(int id, UpdateNotebookDto dto) : IRequest<NotebookViewModel>
    {
        public int Id { get; set; } = id;
        public string InputName { get; set; } = dto.InputName;
        public string? Emoji { get; set; } = dto.Emoji;
        public List<BlockViewModel> Blocks { get; set; } = dto.Blocks;
        public string UserId { get; set; } = dto.UserId;
    }

    public class UpdateNotebookCommandHandler : IRequestHandler<UpdateNotebookCommand, NotebookViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UpdateNotebookCommandHandler(
            IUnitOfWork unitOfWork,
            IMapper mapper
        )
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NotebookViewModel> Handle(UpdateNotebookCommand request, CancellationToken cancellationToken)
        {
            Notebook notebook = await ValidateNotebook(request.Id, request.UserId);

            using TransactionScope scope = new(TransactionScopeAsyncFlowOption.Enabled);
                _mapper.Map(request, notebook, typeof(UpdateNotebookCommand), typeof(Notebook));
                await UpdateNotebookAsync(notebook);
                var notebookVm = _mapper.Map<NotebookViewModel>(notebook);
            scope.Complete();

            return notebookVm;
        }

        private async Task<Notebook> ValidateNotebook(int notebookId, string userId)
        {
            Notebook? notebook = await _unitOfWork.NotebookRepository.GetByIdAsync(notebookId, "Blocks,Collection")
                ?? throw new NotFoundException("Notebook", notebookId);
            notebook.ValidateUser(userId);
            return notebook;
        }

        private async Task UpdateNotebookAsync(Notebook notebook) => await _unitOfWork.NotebookRepository.UpdateAsync(notebook);
    }
}
