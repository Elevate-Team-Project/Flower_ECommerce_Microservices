namespace Catalog_Service.Features.OccasionsFeature.GetAllOccasions
{
    public record OccasionViewModel(
       int Id,
       string Name,
       string? Description,
       string? ImageUrl
   );
}
