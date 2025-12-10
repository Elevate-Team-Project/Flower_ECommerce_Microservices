using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus
{
    public class DeactivateCategoryCommand : IRequest<bool>
    {
        public int Id { get; set; }

        public DeactivateCategoryCommand(int id)
        {
            Id = id;
        }
    }
}
