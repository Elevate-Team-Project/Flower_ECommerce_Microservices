using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategoryStatus
{
    public class DeactivateCategoryCommand : IRequest<RequestResponse<bool>>
    {
        public int Id { get; set; }

        public DeactivateCategoryCommand(int id)
        {
            Id = id;
        }
    }
}
