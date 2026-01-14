using FluentValidation;
using Promotion_Service.Entities;

namespace Promotion_Service.Features.Offers.CreateOffer
{
    public class CreateOfferValidator : AbstractValidator<CreateOfferCommand>
    {
        public CreateOfferValidator()
        {
            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Offer name is required")
                .MaximumLength(200).WithMessage("Offer name cannot exceed 200 characters");

            RuleFor(x => x.DiscountValue)
                .GreaterThan(0).WithMessage("Discount value must be greater than 0");

            RuleFor(x => x.DiscountValue)
                .LessThanOrEqualTo(100)
                .When(x => x.Type == OfferType.Percentage)
                .WithMessage("Percentage discount cannot exceed 100%");

            RuleFor(x => x.StartDate)
                .LessThan(x => x.EndDate)
                .WithMessage("Start date must be before end date");

            RuleFor(x => x.EndDate)
                .GreaterThan(DateTime.UtcNow)
                .WithMessage("End date must be in the future");

            // Target validation
            RuleFor(x => x.ProductId)
                .NotNull()
                .When(x => x.TargetType == OfferTargetType.Product)
                .WithMessage("Product ID is required for product offers");

            RuleFor(x => x.CategoryId)
                .NotNull()
                .When(x => x.TargetType == OfferTargetType.Category)
                .WithMessage("Category ID is required for category offers");

            RuleFor(x => x.OccasionId)
                .NotNull()
                .When(x => x.TargetType == OfferTargetType.Occasion)
                .WithMessage("Occasion ID is required for occasion offers");
        }
    }
}
