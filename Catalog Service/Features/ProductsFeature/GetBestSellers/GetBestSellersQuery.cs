using MediatR;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.ProductsFeature.GetBestSellers
{
    public record GetBestSellersQuery(int Count = 10) : IRequest<RequestResponse<List<BestSellerDto>>>;
}
