using BuildingBlocks.Interfaces;
using Ordering_Service.Features.Cart.ViewShoppingCart.VM;
using Ordering_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Ordering_Service.Entities;

namespace Ordering_Service.Features.Cart.ViewShoppingCart
{
    public class Handlers : IRequestHandler<ViewCartQuery, RequestResponse<CartViewModel>>
    {
        private readonly IBaseRepository<Entities.Cart> _cartRepo;

        private const decimal DELIVERY_FEE = 50; 

        public Handlers(IBaseRepository<Entities.Cart> cartRepo)
        {
            _cartRepo = cartRepo;
        }

        public async Task<RequestResponse<CartViewModel>> Handle(
        ViewCartQuery request,
        CancellationToken cancellationToken)
        {
            var cart = await _cartRepo
                .Get(c => c.UserId == request.UserId)
                .Include(c => c.Items) // Ensure Items are included! Original code used .Select projection which might have skipped Include if done carefully, but here we access c.Items in memory if we don't project.
                // Original used Select directly on IQueryable. Let's see.
                // .Select(c => new CartViewModel ... Items = c.Items.Select...)
                // EF Core handles projection of navigation properties in Select.
                .Select(c => new CartViewModel
                {
                    Items = c.Items.Select(i => new CartItemViewModel
                    {
                        ProductId = i.ProductId,
                        ProductName = i.ProductName,
                        UnitPrice = i.UnitPrice,
                        Quantity = i.Quantity,
                        PictureUrl = i.PictureUrl
                    }).ToList()
                })
                .FirstOrDefaultAsync(cancellationToken);

            if (cart == null || !cart.Items.Any())
            {
                return RequestResponse<CartViewModel>
                    .Success(new CartViewModel
                    {
                        Items = new(),
                        Subtotal = 0,
                        DeliveryFee = 0
                    }, "Cart is empty");
            }

            var subtotal = cart.Items.Sum(i => i.ItemTotal);

            cart.Subtotal = subtotal;
            cart.DeliveryFee = DELIVERY_FEE;

            return RequestResponse<CartViewModel>.Success(cart);
        }

    }
}
