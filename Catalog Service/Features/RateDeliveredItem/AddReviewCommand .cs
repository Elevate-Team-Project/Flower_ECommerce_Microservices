using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.RateDeliveredItem
{
    public class AddReviewCommand : IRequest<RequestResponse<bool>>
    {
        public int ProductId { get; }
        public string UserId { get; }
        public string UserName { get; }
        public int Rating { get; }
        public string? Comment { get; }

        public AddReviewCommand(int productId, string userId, string userName, int rating, string? comment)
        {
            ProductId = productId;
            UserId = userId;
            UserName = userName;
            Rating = rating;
            Comment = comment;
        }
    }
}
