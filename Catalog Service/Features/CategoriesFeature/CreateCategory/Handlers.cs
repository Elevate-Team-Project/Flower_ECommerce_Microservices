using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.CreateCategory
{
    public record CreateCategoryCommand(CreateCategoryDto cat) : IRequest<int>;

}
