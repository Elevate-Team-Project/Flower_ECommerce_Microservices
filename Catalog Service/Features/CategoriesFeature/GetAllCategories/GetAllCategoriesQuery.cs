using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.GetAllCategories
{
    public class GetAllCategoriesQuery : IRequest<RequestResponse<List<CategoryViewModel>>>
    {
    }
}
