using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.ProductsFeature.GetProductDetails
{
    public record  GetProductDetailsQuery(int ProductId) :IRequest<RequestResponse<ProductDetailDto>>
    {
    }
}
