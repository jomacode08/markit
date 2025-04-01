using AutoMapper;
using markit.Application.Contracts.Persistence.Common;
using markit.Application.Exceptions;
using markit.Application.Features.Collections.Queries.ViewModels;
using markit.Domain.Entities;
using MediatR;

namespace markit.Application.Features.Collections.Commands.CreateCollectionCommand
{
    public class CreateCollectionCommand : IRequest<CollectionViewModel>
    {
        public string Name { get; set; } = string.Empty;
        public bool IsMain { get; set; }
        public int CreatorId { get; set; }
        public int? ParentId { get; set; }
        public string? Path { get; set; }
    }

    public class CreateCollectionCommandHandler : IRequestHandler<CreateCollectionCommand, CollectionViewModel>
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public CreateCollectionCommandHandler(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<CollectionViewModel> Handle(CreateCollectionCommand request, CancellationToken cancellationToken)
        {
            // Validations
            ValidateCreatorExistency(request.CreatorId);
            ValidateNameDuplicates(request.Name, request.ParentId);
            if (request.IsMain) ValidateMainCollectionDuplicate(request.CreatorId);

            // Create path
            if (request.ParentId.HasValue)
            {
                int parentId = (int)request.ParentId;

                Collection parent = await _unitOfWork.collectionRepository.GetByIdAsync(parentId) 
                ?? throw new NotFoundException("Collection", parentId);

                request.Path = CreatePath(request, parent);
            }
            else
            {
                request.Path = CreatePath(request);
            }

            // Create collection
            var collection = CreateCollection(request);
            return _mapper.Map<CollectionViewModel>(collection);
        }

        private async void ValidateCreatorExistency(int creatorId)
        {
            var creator = await _unitOfWork.creatorRepository.GetByIdAsync(creatorId)
            ?? throw new NotFoundException("Creator", creatorId);
        }

        private async void ValidateMainCollectionDuplicate(int creatorId)
        {
            var mainCollection = await _unitOfWork.collectionRepository
                .GetAsync(c => c.CreatorId.Equals(creatorId) && c.IsMain.Equals(true));

            if (mainCollection.Any()) throw new CustomValidationException("A main collection is already configured for the user");
        }

        private async void ValidateNameDuplicates(string name, int? parentId)
        {
            var duplicates = await _unitOfWork.collectionRepository
                .GetAsync(c => c.ParentId.Equals(parentId) && c.Name.Equals(name));

            if (duplicates.Any()) throw new CustomValidationException(@$"There's already a collection with the name: { name }");
        }

        private async Task<Collection> CreateCollection(CreateCollectionCommand request)
        {
            var collection = _mapper.Map<Collection>(request);
            _unitOfWork.collectionRepository.AddEntity(collection);
            await _unitOfWork.Complete();

            return collection;
        }

        private static string CreatePath(CreateCollectionCommand request, Collection? parent = null)
        {
            if (parent == null) return @$"/{ request.Name }";

            return $@"{ parent?.Path }/{ request.Name }";
        }
    }
}
