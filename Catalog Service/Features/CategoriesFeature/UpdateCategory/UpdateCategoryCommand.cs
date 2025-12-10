using MediatR;

namespace Catalog_Service.Features.CategoriesFeature.UpdateCategory
{
    public class UpdateCategoryCommand : IRequest<UpdateCategoryDto>
    {
        public int Id { get; set; }
        public UpdateCategoryDto Dto { get; set; }

        public UpdateCategoryCommand(int id, UpdateCategoryDto dto)
        {
            Id = id;
            Dto = dto;
        }
    }
}
