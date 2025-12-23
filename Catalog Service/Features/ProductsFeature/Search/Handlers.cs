using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using LinqKit;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature.Search
{
    public class Handlers : IRequestHandler<SearchProductsQuery, RequestResponse<List<ViewModel>>>
    {
        private readonly IBaseRepository<Product> _productRepo;

        public Handlers(IBaseRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<RequestResponse<List<ViewModel>>> Handle(
    SearchProductsQuery request,
    CancellationToken cancellationToken)
        {
            var f = request.Filters;

            var predicate = PredicateBuilder.New<Product>(p => p.IsAvailable);

            if (!string.IsNullOrWhiteSpace(f.Name))
                predicate = predicate.And(p =>
                    p.Name.Contains(f.Name));

            if (f.MinPrice.HasValue && f.MinPrice > 0)
                predicate = predicate.And(p =>
                    p.Price >= f.MinPrice.Value);

            if (f.MaxPrice.HasValue && f.MaxPrice > 0)
                predicate = predicate.And(p =>
                    p.Price <= f.MaxPrice.Value);

            if (f.CategoryIds?.Any(id => id > 0) == true)
            {
                var validCategoryIds = f.CategoryIds
                    .Where(id => id > 0)
                    .ToList();

                predicate = predicate.And(p =>
                    validCategoryIds.Contains(p.CategoryId));
            }

            if (f.OccasionIds?.Any(id => id > 0) == true)
            {
                var validOccasionIds = f.OccasionIds
                    .Where(id => id > 0)
                    .ToList();

                predicate = predicate.And(p =>
                    p.ProductOccasions.Any(po =>
                        validOccasionIds.Contains(po.OccasionId)));
            }

            var products = await _productRepo
                .Get(predicate)
                .AsExpandable()
                .Select(p => new ViewModel
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    IsAvailable = p.IsAvailable,

                    CategoryId = p.CategoryId,
                    CategoryName = p.Category.Name,

                    BrandId = p.BrandId,
                    BrandName = p.Brand != null ? p.Brand.Name : null,

                    MainImageUrl = p.Images
                        .Where(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault(),

                    OccasionNames = p.ProductOccasions
                        .Select(po => po.Occasion.Name)
                        .ToList()
                })
                .ToListAsync(cancellationToken);

            if (!products.Any())
                return RequestResponse<List<ViewModel>>
                    .Success(new List<ViewModel>(),
                        "No products found matching your search criteria");

            return RequestResponse<List<ViewModel>>
                .Success(products, "Products fetched successfully");
        }


    }
}
