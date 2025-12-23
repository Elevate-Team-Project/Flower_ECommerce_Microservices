using FluentValidation;

namespace Ordering_Service.Features.Orders.CreateOrder
{
    public class CreateOrderValidator : AbstractValidator<CreateOrderCommand>
    {
        public CreateOrderValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.Items)
                .NotEmpty().WithMessage("Order must contain at least one item");

            RuleForEach(x => x.Items).ChildRules(item =>
            {
                item.RuleFor(i => i.ProductId)
                    .GreaterThan(0).WithMessage("Product ID must be greater than 0");

                item.RuleFor(i => i.ProductName)
                    .NotEmpty().WithMessage("Product name is required");

                item.RuleFor(i => i.UnitPrice)
                    .GreaterThan(0).WithMessage("Unit price must be greater than 0");

                item.RuleFor(i => i.Quantity)
                    .GreaterThan(0).WithMessage("Quantity must be greater than 0");
            });

            RuleFor(x => x.ShippingAddress)
                .NotEmpty().WithMessage("Shipping address is required");

            RuleFor(x => x.BillingAddress)
                .NotEmpty().WithMessage("Billing address is required");

            RuleFor(x => x.PaymentMethod)
                .NotEmpty().WithMessage("Payment method is required");
        }
    }
}
