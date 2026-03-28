﻿using FluentValidation;

namespace markit.Application.Features.Collections.Commands.MoveCollectionCommand
{
    public class MoveCollectionCommandValidator : AbstractValidator<MoveCollectionCommand>
    {
        public MoveCollectionCommandValidator()
        {
            RuleFor(c => c.CollectionId).NotEmpty().WithMessage("The collectionId is required");
            RuleFor(c => c.ParentId).NotEmpty().WithMessage("The parentId is required");
        }
    }
}
