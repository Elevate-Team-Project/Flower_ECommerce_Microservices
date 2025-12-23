using FluentValidation;

namespace Catalog_Service.Features.OccasionsFeature.CreateOccasion
{
    public class CreateOccasionValidator : AbstractValidator<CreateOccasionDto>
    {
        public CreateOccasionValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Occasion name is required.")
                .MaximumLength(100).WithMessage("Name must not exceed 100 characters.");
        }
    }
}
