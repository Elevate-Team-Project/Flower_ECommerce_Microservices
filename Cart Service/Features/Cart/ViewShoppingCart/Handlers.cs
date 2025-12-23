using BuildingBlocks.Interfaces;
using Cart_Service.Features.Cart.ViewShoppingCart.VM;
using Cart_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Cart_Service.Features.Cart.ViewShoppingCart
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
