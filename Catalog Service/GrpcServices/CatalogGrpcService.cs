using BuildingBlocks.Grpc;
using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.GrpcServices;

/// <summary>
/// gRPC service implementation for Catalog operations.
/// Provides high-performance inter-service communication for product queries.
/// </summary>
public class CatalogGrpcService : CatalogGrpc.CatalogGrpcBase
{
    private readonly IBaseRepository<Product> _productRepository;
    private readonly ILogger<CatalogGrpcService> _logger;

    public CatalogGrpcService(
        IBaseRepository<Product> productRepository,
        ILogger<CatalogGrpcService> logger)
    {
        _productRepository = productRepository;
        _logger = logger;
    }

    public override async Task<ProductResponse> GetProduct(
        GetProductRequest request, 
        ServerCallContext context)
    {
        try
        {
            var product = await _productRepository
                .GetAll()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, context.CancellationToken);

            if (product == null)
            {
                return new ProductResponse
                {
                    Success = false,
                    ErrorMessage = $"Product with ID {request.ProductId} not found"
                };
            }

            return new ProductResponse
            {
                Success = true,
                Product = MapToDto(product)
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting product {ProductId}", request.ProductId);
            return new ProductResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public override async Task<ProductsResponse> GetProductsByIds(
        GetProductsByIdsRequest request, 
        ServerCallContext context)
    {
        try
        {
            var productIds = request.ProductIds.ToList();
            var products = await _productRepository
                .GetAll()
                .Include(p => p.Category)
                .Include(p => p.Images)
                .Where(p => productIds.Contains(p.Id))
                .ToListAsync(context.CancellationToken);

            var response = new ProductsResponse { Success = true };
            response.Products.AddRange(products.Select(MapToDto));
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting products by IDs");
            return new ProductsResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    public override async Task<AvailabilityResponse> CheckProductAvailability(
        CheckAvailabilityRequest request, 
        ServerCallContext context)
    {
        try
        {
            var product = await _productRepository
                .GetAll()
                .FirstOrDefaultAsync(p => p.Id == request.ProductId, context.CancellationToken);

            if (product == null)
            {
                return new AvailabilityResponse
                {
                    Success = false,
                    IsAvailable = false,
                    ErrorMessage = $"Product with ID {request.ProductId} not found"
                };
            }

            var isAvailable = product.IsAvailable && product.StockQuantity >= request.Quantity;
            return new AvailabilityResponse
            {
                Success = true,
                IsAvailable = isAvailable
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking availability for product {ProductId}", request.ProductId);
            return new AvailabilityResponse
            {
                Success = false,
                ErrorMessage = ex.Message
            };
        }
    }

    private static ProductDto MapToDto(Product product)
    {
        var dto = new ProductDto
        {
            Id = product.Id,
            Name = product.Name ?? string.Empty,
            Description = product.Description ?? string.Empty,
            Price = (double)product.Price,
            StockQuantity = product.StockQuantity,
            IsAvailable = product.IsAvailable,
            CategoryId = product.CategoryId,
            CategoryName = product.Category?.Name ?? string.Empty
        };

        // Get first image URL if available
        if (product.Images?.Any() == true)
        {
            dto.ImageUrl = product.Images.FirstOrDefault()?.ImageUrl ?? string.Empty;
        }

        return dto;
    }
}
