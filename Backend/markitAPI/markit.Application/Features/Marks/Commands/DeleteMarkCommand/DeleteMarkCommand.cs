using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Marks.Commands.DeleteMarkCommand
{
    public class DeleteMarkCommand : IRequest<bool>
    {
        public int Id { get; set; }
    }

    public class DeleteMarkCommandHandler : IRequestHandler<DeleteMarkCommand, bool>
    {
        private readonly IUnitOfWork _unitOfWork;

        public DeleteMarkCommandHandler(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
        }
        
        public async Task<bool> Handle(DeleteMarkCommand request, CancellationToken cancellationToken)
        {
            // Validate the existence of the mark
            Mark mark = await _unitOfWork.markRepository.GetByIdAsync(request.Id, "Links")
                ?? throw new NotFoundException("Mark", request.Id);

            // Soft delete the links
            if (mark.Links != null && mark.Links.Count > 0)
            {
                _unitOfWork.Repository<Link>()
                    .SoftDeleteRangeEntity(mark.Links.ToList());
            }

            // Soft delete the mark
            _unitOfWork.markRepository.SoftDeleteEntity(mark);

            // Complete the transaction
            await _unitOfWork.Complete();
            return true;
        }
    }
}
