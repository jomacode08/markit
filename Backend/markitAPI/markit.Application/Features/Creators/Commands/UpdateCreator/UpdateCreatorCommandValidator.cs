using FluentValidation;
using System.Globalization;

namespace markit.Application.Features.Creators.Commands.UpdateCreator
{
    public class UpdateCreatorCommandValidator : AbstractValidator<UpdateCreatorCommand>
    {
        private static DateOnly MinDate => DateOnly.FromDateTime(DateTime.Now.AddYears(-100));
        private static DateOnly MaxDate => DateOnly.FromDateTime(DateTime.Now.AddDays(1));

        public UpdateCreatorCommandValidator() {
            RuleFor(c => c.Id).NotEmpty().WithMessage("The Id is required");
            RuleFor(c => c.FirstName).NotEmpty().WithMessage("The first name is required");
            RuleFor(c => c.LastName).NotEmpty().WithMessage("The last name is required");
            RuleFor(c => c.BirthDate).NotEmpty().WithMessage("The birth date is required");
            RuleFor(c => c.Gender).NotEmpty().WithMessage("The gender is required");

            RuleFor(c => c.FirstName).MaximumLength(100).WithMessage("The maximun length of first name is 100");
            RuleFor(c => c.LastName).MaximumLength(100).WithMessage("The maximun length of last name is 100");

            // Min birth date
            RuleFor(c => DateOnly.Parse(c.BirthDate))
                .GreaterThan(MinDate)
                .WithMessage($"The birth date must be greater than {MinDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}");

            // Max birth date
            RuleFor(c => DateOnly.Parse(c.BirthDate))
                .LessThan(MaxDate)
                .WithMessage($"The birth date must be less than {MaxDate.ToString("dd MMMM yyyy", CultureInfo.InvariantCulture)}");
        }
    }
}
