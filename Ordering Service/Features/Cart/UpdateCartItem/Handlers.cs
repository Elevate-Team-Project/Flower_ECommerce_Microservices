using Ordering_Service.Entities; // Fixed namespace
using BuildingBlocks.Interfaces;
using Ordering_Service.Features.Shared;
using MediatR;

namespace Ordering_Service.Features.Cart.UpdateCartItem
{
    public class Handlers : IRequestHandler<UpdateCartItemCommand, RequestResponse<UpdateCartItemDto>>
    {
        private readonly IBaseRepository<Entities.CartItem> baseRepository;
        private readonly IUnitOfWork _UOW;

        public Handlers(IBaseRepository<Entities.CartItem> baseRepository, IUnitOfWork UOW)
        {
            this.baseRepository = baseRepository;
            _UOW = UOW;
        }

        public async Task<RequestResponse<UpdateCartItemDto>> Handle(UpdateCartItemCommand request, CancellationToken cancellationToken)
        {
            var cartItem = baseRepository
                                                 .Get(c => c.Id == request.CartId)
                                                 .FirstOrDefault();
                                                  
            if (cartItem == null)
            {
                return (RequestResponse<UpdateCartItemDto>.Fail("CartItem not found"));
            }
            if (request.newquantity == 0)
            {
                baseRepository.Delete(cartItem);
                await _UOW.SaveChangesAsync(cancellationToken);

                return (RequestResponse<UpdateCartItemDto>.Fail("Cart Item Is Deleted"));
            }

            cartItem.Quantity = request.newquantity;
            baseRepository.Update(cartItem);
            await _UOW.SaveChangesAsync(cancellationToken);
            var updatedCartItemDto = new UpdateCartItemDto
            {
                CartItemId = cartItem.Id,
                ProductId = cartItem.ProductId,
                ProductName = cartItem.ProductName,
                UnitPrice = cartItem.UnitPrice,
                Quantity = cartItem.Quantity,
                PictureUrl = cartItem.PictureUrl
            };
            return (RequestResponse<UpdateCartItemDto>.Success(updatedCartItemDto));

        }
    }
}
