using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.ProductsFeature.GetBestSellers
{
    public class GetBestSellersHandler : IRequestHandler<GetBestSellersQuery, RequestResponse<List<BestSellerDto>>>
    {
        private readonly IBaseRepository<Product> _productRepo;

        public GetBestSellersHandler(IBaseRepository<Product> productRepo)
        {
            _productRepo = productRepo;
        }

        public async Task<RequestResponse<List<BestSellerDto>>> Handle(
            GetBestSellersQuery request,
            CancellationToken cancellationToken)
        {
           
            var bestSellers = await _productRepo
                .Get(p => !p.IsDeleted && p.IsAvailable && p.StockQuantity > 0)
                .OrderByDescending(p => p.Reviews.Where(r => !r.IsDeleted).Any() 
                    ? p.Reviews.Where(r => !r.IsDeleted).Average(r => r.Rating) 
                    : 0)
                .ThenByDescending(p => p.Reviews.Where(r => !r.IsDeleted).Count())
                .ThenByDescending(p => p.CreatedAt)
                .Take(request.Count)
                .Select(p => new BestSellerDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Description = p.Description,
                    Price = p.Price,
                    DiscountedPrice = p.PriceHistories
                        .OrderByDescending(ph => ph.CreatedAt)
                        .Select(ph => (decimal?)ph.NewPrice)
                        .FirstOrDefault(),
                    DiscountPercentage = p.PriceHistories
                        .OrderByDescending(ph => ph.CreatedAt)
                        .Select(ph => ph.NewPrice < p.Price 
                            ? Math.Round((p.Price - ph.NewPrice) / p.Price * 100, 2) 
                            : (decimal?)null)
                        .FirstOrDefault(),
                    MainImage = p.Images
                        .Where(i => !i.IsDeleted && i.IsMain)
                        .Select(i => i.ImageUrl)
                        .FirstOrDefault() 
                        ?? p.Images
                            .Where(i => !i.IsDeleted)
                            .Select(i => i.ImageUrl)
                            .FirstOrDefault(),
                    Images = p.Images
                        .Where(i => !i.IsDeleted)
                        .OrderByDescending(i => i.IsMain)
                        .Select(i => i.ImageUrl)
                        .ToList(),
                    CategoryName = p.Category != null ? p.Category.Name : "Uncategorized",
                    BrandName = p.Brand != null ? p.Brand.Name : null,
                    InStock = p.StockQuantity > 0 && p.IsAvailable,
                    StockQuantity = p.StockQuantity,
                    AverageRating = p.Reviews.Where(r => !r.IsDeleted).Any() 
                        ? Math.Round(p.Reviews.Where(r => !r.IsDeleted).Average(r => r.Rating), 1) 
                        : 0,
                    ReviewCount = p.Reviews.Where(r => !r.IsDeleted).Count()
                })
                .ToListAsync(cancellationToken);

            return RequestResponse<List<BestSellerDto>>.Success(bestSellers);
        }
    }
}
