using Catalog_Service.Features.CategoriesFeature.GetAllCategories;
using Catalog_Service.Features.OccasionsFeature.UpdateOccasion;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.GetActiveCategoryFeature
{
    public record GetAllActiveCategoriesQuery()
    : IRequest<RequestResponse<List<CategoryactiveViewModel>>>;
}
