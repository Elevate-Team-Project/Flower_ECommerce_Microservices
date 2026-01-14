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

        public DeleteProductHandler(IBaseRepository<Product> productRepo, IUnitOfWork unitOfWork)
        {
            _productRepo = productRepo;
            _unitOfWork = unitOfWork;
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

            // Check if product can be deleted (e.g., active orders check should be here or handled via soft delete)
            // For now, implementing strict delete as per requirements, but ensuring cascade delete works or manual clear
            // EF Core should handle cascade if configured, but safe to clear manual:
            
            // Hard delete logic:
             _productRepo.HardDelete(product);
             await _unitOfWork.SaveChangesAsync(cancellationToken);

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
