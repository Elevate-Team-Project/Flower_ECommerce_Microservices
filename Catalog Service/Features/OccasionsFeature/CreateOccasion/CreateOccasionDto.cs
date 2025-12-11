using System.ComponentModel.DataAnnotations;

namespace Catalog_Service.Features.OccasionsFeature.CreateOccasion
{
    public class CreateOccasionDto
    {
        
        public string Name { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
