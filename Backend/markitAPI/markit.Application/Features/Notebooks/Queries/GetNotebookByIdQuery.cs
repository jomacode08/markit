using AutoMapper;
using markit.Application.Common.Helpers;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Notebooks.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Notebooks.Queries
{
    public class GetNotebookByIdQuery(int id, string userId) : IRequest<NotebookViewModel>
    {
        public int Id { get; set; } = id;
        public string UserId {  get; set; } = userId;
    }

    public class GetNotebookByIdQueryHandler : IRequestHandler<GetNotebookByIdQuery, NotebookViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public GetNotebookByIdQueryHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<NotebookViewModel> Handle(GetNotebookByIdQuery request, CancellationToken cancellationToken)
        {
            Notebook notebook = await _unitOfWork.NotebookRepository.GetWithOrderedBlocks(request.Id)
                ?? throw new NotFoundException("Notebook", request.Id);
            notebook.ValidateUser(request.UserId);
            return _mapper.Map<NotebookViewModel>(notebook);
        }
    }
}
