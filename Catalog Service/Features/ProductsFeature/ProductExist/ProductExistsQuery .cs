using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.ProductsFeature.ProductExist
{
    public class ProductExistsQuery : IRequest<RequestResponse<bool>>
    {
        public ProductExistsDto Dto { get; }

        public ProductExistsQuery(ProductExistsDto dto)
        {
            Dto = dto;
        }
    }
}
