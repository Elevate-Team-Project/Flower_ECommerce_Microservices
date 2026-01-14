using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoryWithProduct.ViewCategoryProducts
{
    public class ViewCategoryProductsQuery : IRequest<RequestResponse<List<DTO>>>
    {
        public int CategoryId { get; }

        public ViewCategoryProductsQuery(int categoryId)
        {
            CategoryId = categoryId;
        }
    }
}
