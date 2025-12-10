using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus
{
    public class ActivateCategoryCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public ActivateCategoryCommand(int id)
        {
            Id = id;
        }
    }
}
