using FluentValidation;

namespace Delivery_Service.Features.Addresses.CreateAddress
{
    public class CreateAddressValidator : AbstractValidator<CreateAddressCommand>
    {
        public CreateAddressValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.AddressLabel)
                .NotEmpty().WithMessage("Address label is required")
                .MaximumLength(50).WithMessage("Address label must not exceed 50 characters");

            RuleFor(x => x.FullName)
                .NotEmpty().WithMessage("Full name is required")
                .MaximumLength(100).WithMessage("Full name must not exceed 100 characters");

            RuleFor(x => x.Phone)
                .NotEmpty().WithMessage("Phone is required")
                .Matches(@"^[\d\s\+\-\(\)]+$").WithMessage("Invalid phone number format");

            RuleFor(x => x.Governorate)
                .NotEmpty().WithMessage("Governorate is required")
                .MaximumLength(100).WithMessage("Governorate must not exceed 100 characters");

            RuleFor(x => x.City)
                .NotEmpty().WithMessage("City is required")
                .MaximumLength(100).WithMessage("City must not exceed 100 characters");

            RuleFor(x => x.Street)
                .NotEmpty().WithMessage("Street is required")
                .MaximumLength(200).WithMessage("Street must not exceed 200 characters");

            RuleFor(x => x.Country)
                .NotEmpty().WithMessage("Country is required");

            // Map coordinates validation (if provided, both must be valid)
            When(x => x.Latitude.HasValue || x.Longitude.HasValue, () =>
            {
                RuleFor(x => x.Latitude)
                    .NotNull().WithMessage("Latitude is required when Longitude is provided")
                    .InclusiveBetween(-90, 90).WithMessage("Latitude must be between -90 and 90");

                RuleFor(x => x.Longitude)
                    .NotNull().WithMessage("Longitude is required when Latitude is provided")
                    .InclusiveBetween(-180, 180).WithMessage("Longitude must be between -180 and 180");
            });

            RuleFor(x => x.Building)
                .MaximumLength(50).When(x => x.Building != null);

            RuleFor(x => x.Floor)
                .MaximumLength(20).When(x => x.Floor != null);

            RuleFor(x => x.Apartment)
                .MaximumLength(20).When(x => x.Apartment != null);

            RuleFor(x => x.Landmark)
                .MaximumLength(200).When(x => x.Landmark != null);
        }
    }
}
