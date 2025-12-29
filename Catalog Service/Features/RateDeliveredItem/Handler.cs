using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Catalog_Service.Features.RateDeliveredItem
{
    public class Handler : IRequestHandler<AddReviewCommand, RequestResponse<bool>>
    {
        private readonly IBaseRepository<Product> _productRepo;
        private readonly IBaseRepository<ProductReview> _reviewRepo;
        private readonly IUnitOfWork _uow;

        public Handler(
            IBaseRepository<Product> productRepo,
            IBaseRepository<ProductReview> reviewRepo,
            IUnitOfWork uow)
        {
            _productRepo = productRepo;
            _reviewRepo = reviewRepo;
            _uow = uow;
        }

        public async Task<RequestResponse<bool>> Handle(AddReviewCommand request, CancellationToken cancellationToken)
        {
            
            var product = await _productRepo
                .Get(p => p.Id == request.ProductId)
                .FirstOrDefaultAsync(cancellationToken);

            if (product == null)
                return RequestResponse<bool>.Fail("Product not found");

            var alreadyReviewed = await _reviewRepo
                .Get(r => r.ProductId == request.ProductId && r.UserId == request.UserId)
                .AnyAsync(cancellationToken);

            if (alreadyReviewed)
                return RequestResponse<bool>.Fail("You already reviewed this product");

            if (request.Rating <= 3 && string.IsNullOrWhiteSpace(request.Comment))
                return RequestResponse<bool>.Fail("Please provide a comment when rating 3 or below");

            var review = new ProductReview
            {
                ProductId = request.ProductId,
                UserId = request.UserId,
                UserName = request.UserName,
                Rating = request.Rating,
                Comment = request.Comment,
                ReviewedAt = DateTime.UtcNow
            };

            await _reviewRepo.AddAsync(review);

            var allRatings = await _reviewRepo
                .Get(r => r.ProductId == request.ProductId)
                .Select(r => r.Rating)
                .ToListAsync(cancellationToken);

            product.TotalReviews = allRatings.Count;
            product.AverageRating = (decimal)allRatings.Average();

            _productRepo.Update(product);

            await _uow.SaveChangesAsync(cancellationToken);

            return RequestResponse<bool>.Success(true, "Review added successfully");
        }
    }
}
