using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Catalog_Service.Entities;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Features.Shared;
using System.Net.Http;
using System.Net.Http.Json;

namespace Catalog_Service.Features.ProductsFeature.DeleteProduct
{
    // --- Command ---
    public record DeleteProductCommand(int Id) : IRequest<EndpointResponse<bool>>;

    // --- Handler ---
    public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<DeleteProductHandler> _logger;

        public DeleteProductHandler(
            IBaseRepository<Product> productRepo, 
            IUnitOfWork unitOfWork,
            IHttpClientFactory httpClientFactory,
            ILogger<DeleteProductHandler> logger)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
            _httpClientFactory = httpClientFactory;
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
                var httpClient = _httpClientFactory.CreateClient("OrderingService");
                var response = await httpClient.GetAsync($"api/orders/check-product/{request.Id}", cancellationToken);
                if (response.IsSuccessStatusCode)
                {
                    var checkResult = await response.Content.ReadFromJsonAsync<ActiveOrderCheckResponse>(cancellationToken: cancellationToken);
                    if (checkResult != null && checkResult.HasActiveOrders)
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

        private class ActiveOrderCheckResponse
        {
            public bool Success { get; set; }
            public bool HasActiveOrders { get; set; }
            public int ActiveOrderCount { get; set; }
            public string Message { get; set; } = string.Empty;
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
