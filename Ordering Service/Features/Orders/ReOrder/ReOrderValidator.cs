using FluentValidation;

namespace Ordering_Service.Features.Orders.ReOrder
{
    public class ReOrderValidator : AbstractValidator<ReOrderCommand>
    {
        public ReOrderValidator()
        {
            RuleFor(x => x.OriginalOrderId)
                .GreaterThan(0)
                .WithMessage("Original order ID must be a valid positive number");

            RuleFor(x => x.Items)
                .NotEmpty()
                .WithMessage("Items list is required");

            RuleFor(x => x.Items)
                .Must(items => items != null && items.Any(i => i.Quantity > 0))
                .WithMessage("At least one item must have a quantity greater than 0");

            RuleForEach(x => x.Items)
                .ChildRules(item =>
                {
                    item.RuleFor(i => i.ProductId)
                        .GreaterThan(0)
                        .WithMessage("Product ID must be a valid positive number");

                    item.RuleFor(i => i.Quantity)
                        .GreaterThanOrEqualTo(0)
                        .WithMessage("Quantity cannot be negative");

                    item.RuleFor(i => i.Quantity)
                        .LessThanOrEqualTo(100)
                        .WithMessage("Quantity cannot exceed 100 items");
                });

            // Either DeliveryAddressId or ShippingAddress should be provided for delivery
            RuleFor(x => x)
                .Must(x => x.DeliveryAddressId.HasValue || !string.IsNullOrEmpty(x.ShippingAddress))
                .WithMessage("Either a delivery address ID or shipping address must be provided")
                .When(x => x.DeliveryAddressId.HasValue || !string.IsNullOrEmpty(x.ShippingAddress));
        }
    }
}
