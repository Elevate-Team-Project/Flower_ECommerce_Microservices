using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature.GetInventorySummary
{
    // --- Query ---
    public record GetInventorySummaryQuery(string? StatusFitler = null) : IRequest<EndpointResponse<List<InventorySummaryDto>>>;

    // --- DTO ---
    public record InventorySummaryDto(
        int ProductId,
        string ProductName,
        int StockQuantity,
        int MinStock,
        int MaxStock,
        string Status
    );

    // --- Handler ---
    public class GetInventorySummaryHandler : IRequestHandler<GetInventorySummaryQuery, EndpointResponse<List<InventorySummaryDto>>>
    {
        private readonly IBaseRepository<Product> _productRepo;

        public GetInventorySummaryHandler(IBaseRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<EndpointResponse<List<InventorySummaryDto>>> Handle(GetInventorySummaryQuery request, CancellationToken cancellationToken)
        {
            var query = _productRepo.GetAll();

            // Client-side evaluation might be needed for computed Status if complex, but simple comparison works in SQL usually.
            // Let's fetch basic info first. 
            // Better to project to DTO.

            var products = await query.Select(p => new 
            {
                p.Id,
                p.Name,
                p.StockQuantity,
                p.MinStock,
                p.MaxStock
            }).ToListAsync(cancellationToken);

            var inventoryList = products.Select(p => new InventorySummaryDto(
                p.Id,
                p.Name,
                p.StockQuantity,
                p.MinStock,
                p.MaxStock,
                DetermineStatus(p.StockQuantity, p.MinStock)
            )).ToList();

            if (!string.IsNullOrEmpty(request.StatusFitler))
            {
                inventoryList = inventoryList.Where(i => i.Status.Equals(request.StatusFitler, StringComparison.OrdinalIgnoreCase)).ToList();
            }

            return EndpointResponse<List<InventorySummaryDto>>.SuccessResponse(inventoryList);
        }

        private string DetermineStatus(int quantity, int minStock)
        {
            if (quantity <= 0) return "Out of Stock";
            if (quantity <= minStock) return "Low Stock";
            return "In Stock";
        }
    }

    // --- Endpoints ---
    public static class Endpoints
    {
        public static void MapGetInventorySummaryEndpoints(this IEndpointRouteBuilder app)
        {
            app.MapGet("/api/inventory", async (string? status, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetInventorySummaryQuery(status));
                return Results.Ok(result);
            })
            .WithTags("Inventory")
            .WithName("GetInventorySummary");
        }
    }
}
