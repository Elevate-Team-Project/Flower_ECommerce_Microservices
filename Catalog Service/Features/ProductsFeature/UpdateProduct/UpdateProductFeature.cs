using BuildingBlocks.Interfaces;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.ProductsFeature.UpdateProduct
{
    // --- Commands / DTOs ---

    public class UpdateProductCommand : IRequest<EndpointResponse<bool>>
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public decimal Price { get; set; }
        public int CategoryId { get; set; }
        public int? BrandId { get; set; }
        public List<int> OccasionIds { get; set; } = new();
        public List<string> ImageUrls { get; set; } = new();
        public List<ProductSpecificationDto> Specifications { get; set; } = new();
    }

    public class ProductSpecificationDto
    {
        public string Name { get; set; } = string.Empty;
        public string Value { get; set; } = string.Empty;
    }

    // --- Validator ---

    public class UpdateProductValidator : AbstractValidator<UpdateProductCommand>
    {
        public UpdateProductValidator()
        {
            RuleFor(x => x.Id).GreaterThan(0);
            RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
            RuleFor(x => x.Description).NotEmpty();
            RuleFor(x => x.Price).GreaterThan(0);
            RuleFor(x => x.CategoryId).GreaterThan(0);
        }
    }

    // --- Handler ---

    public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<Category> _categoryRepo;
        private readonly IBaseRepository<Occasion> _occasionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public UpdateProductHandler(
            IBaseRepository<Product> productRepo,
            IBaseRepository<Category> categoryRepo,
            IBaseRepository<Occasion> occasionRepo,
            IUnitOfWork unitOfWork)
        {
            _productRepo = productRepo;
            _categoryRepo = categoryRepo;
            _occasionRepo = occasionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<bool>> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Get existing product
            var product = await _productRepo.Get(p => p.Id == request.Id)
                .Include(p => p.Images)
                .Include(p => p.Specifications)
                .Include(p => p.ProductOccasions)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
                return EndpointResponse<bool>.ErrorResponse("Product not found", 404);

            // 2. Validate Name Uniqueness (if changed)
            if (product.Name != request.Name)
            {
                var exists = await _productRepo.Get(p => p.Name == request.Name).AnyAsync(cancellationToken);
                if (exists)
                    return EndpointResponse<bool>.ErrorResponse("Product with this name already exists", 409);
            }

            // 3. Validate Category
            if (product.CategoryId != request.CategoryId)
            {
                var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
                if (category == null) return EndpointResponse<bool>.ErrorResponse("Invalid Category Id", 400);
            }

            // 4. Validate Occasions
            var occasions = await _occasionRepo.Get(o => request.OccasionIds.Contains(o.Id))
                .ToListAsync(cancellationToken);
            if (occasions.Count != request.OccasionIds.Count)
                return EndpointResponse<bool>.ErrorResponse("Invalid Occasion Ids", 400);

            // 5. Update Basic Fields
            product.Name = request.Name;
            product.Description = request.Description;
            product.Price = request.Price;
            product.CategoryId = request.CategoryId;
            product.BrandId = request.BrandId;

            // 6. Update Collections (Simple replace logic for now - clear and re-add)
            // Images
            product.Images.Clear();
            foreach (var url in request.ImageUrls)
            {
                product.Images.Add(new ProductImage { ImageUrl = url, ProductId = product.Id });
            }

            // Specs
            product.Specifications.Clear();
            foreach (var spec in request.Specifications)
            {
                product.Specifications.Add(new ProductSpecification { SpecKey = spec.Name, SpecValue = spec.Value, ProductId = product.Id });
            }

            // Occasions
            product.ProductOccasions.Clear();
            foreach (var occ in occasions)
            {
                product.ProductOccasions.Add(new ProductOccasion { OccasionId = occ.Id, ProductId = product.Id });
            }

            _productRepo.Update(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, "Product updated successfully");
        }
    }

    // --- Endpoint ---

    public static class Endpoints
    {
        public static void MapUpdateProductEndpoint(this IEndpointRouteBuilder app)
        {
            app.MapPut("/api/products", async ([FromBody] UpdateProductCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                if (!result.IsSuccess) return Results.BadRequest(result);
                return Results.Ok(result);
            })
            .WithTags("Products")
            .WithName("UpdateProduct");
        }
    }
}
