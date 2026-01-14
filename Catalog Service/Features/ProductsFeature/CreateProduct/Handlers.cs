using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.ProductsFeature.CreateProduct
{
    public class CreateProductHandler : IRequestHandler<CreateProductCommand, EndpointResponse<int>>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<Category> _categoryRepo;
        private readonly IBaseRepository<Occasion> _occasionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public CreateProductHandler(
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

        public async Task<EndpointResponse<int>> Handle(CreateProductCommand request, CancellationToken cancellationToken)
        {
            // 1. Validate Category Exists
            var category = await _categoryRepo.GetByIdAsync(request.CategoryId);
            if (category == null)
                return EndpointResponse<int>.ErrorResponse("Invalid Category Id", 400);

            // 2. Validate Occasions Exist
            var occasions = await _occasionRepo.Get(o => request.OccasionIds.Contains(o.Id))
                .ToListAsync(cancellationToken);

            if (occasions.Count != request.OccasionIds.Count)
                return EndpointResponse<int>.ErrorResponse("One or more invalid Occasion Ids", 400);

            // 3. Check for Duplicate Name
            var exists = await _productRepo.Get(p => p.Name == request.Name).AnyAsync(cancellationToken);
            if (exists)
                return EndpointResponse<int>.ErrorResponse("Product name already exists", 409);

            // 4. Map to Entity
            var product = new Product
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                StockQuantity = request.StockQuantity,
                MinStock = request.MinStock,
                MaxStock = request.MaxStock,
                IsAvailable = request.IsAvailable,
                CategoryId = request.CategoryId,
                BrandId = request.BrandId,
                Images = request.ImageUrls.Select(url => new ProductImage { ImageUrl = url }).ToList(),
                Specifications = request.Specifications.Select(s => new ProductSpecification { SpecKey = s.Name, SpecValue = s.Value }).ToList(),
                ProductOccasions = occasions.Select(o => new ProductOccasion { OccasionId = o.Id }).ToList()
            };

            await _productRepo.AddAsync(product);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<int>.SuccessResponse(product.Id, "Product created successfully", 201);
        }
    }
}
