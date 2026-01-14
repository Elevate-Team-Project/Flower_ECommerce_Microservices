using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.ProductsFeature.DeleteProduct
{
    // --- Command ---
    public record DeleteProductCommand(int Id) : IRequest<EndpointResponse<bool>>;

    // --- Handler ---
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly BuildingBlocks.Grpc.OrderingGrpc.OrderingGrpcClient _orderingGrpcClient;
        private readonly ILogger<DeleteProductHandler> _logger;

        public DeleteProductHandler(
            IBaseRepository<Product> productRepo, 
            IUnitOfWork unitOfWork,
            BuildingBlocks.Grpc.OrderingGrpc.OrderingGrpcClient orderingGrpcClient,
            ILogger<DeleteProductHandler> logger)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _orderingGrpcClient = orderingGrpcClient;
            _logger = logger;
        }

        public async Task<EndpointResponse<bool>> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
        {
            var product = await _productRepo.Get(p => p.Id == request.Id)
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .Include(p => p.ProductOccasions)
                .Include(p => p.Reviews)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
                return EndpointResponse<bool>.ErrorResponse("Product not found", 404);

            // US-A11: Check if product is part of any active order before deletion
            try
            {
                var checkResult = await _orderingGrpcClient.CheckProductInActiveOrdersAsync(
                    new BuildingBlocks.Grpc.CheckProductRequest { ProductId = request.Id },
                    cancellationToken: cancellationToken);

                if (checkResult.HasActiveOrders)
                {
                    _logger.LogWarning(
                        "Attempted to delete product {ProductId} which is in {Count} active orders",
                        request.Id, checkResult.ActiveOrderCount);

                    return EndpointResponse<bool>.ErrorResponse(
                        $"Cannot delete product. It is part of {checkResult.ActiveOrderCount} active order(s). " +
                        "Please wait until all orders containing this product are delivered or cancelled.",
                        400);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to check active orders for product {ProductId}. Blocking deletion for safety.", request.Id);
                return EndpointResponse<bool>.ErrorResponse(
                    "Unable to verify product order status. Please try again later.", 503);
            }
            
            // Safe to delete - no active orders
            _productRepo.HardDelete(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            _logger.LogInformation("Product {ProductId} deleted successfully", request.Id);
            return EndpointResponse<bool>.SuccessResponse(true, "Product deleted successfully");
        }
    }

    // --- Endpoint ---
    public static class Endpoints
    {
        public static void MapDeleteProductEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapDelete("/api/products/{id}", async (int id, IMediator mediator) =>
            {
                var result = await mediator.Send(new DeleteProductCommand(id));
                if (!result.IsSuccess) return Results.BadRequest(result);
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("DeleteProduct");
        }
    }
}
