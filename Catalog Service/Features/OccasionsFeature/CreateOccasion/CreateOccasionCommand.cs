using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.CreateOccasion
{
    public class CreateOccasionCommand : IRequest<CreateOccasionDto>
    {
        public CreateOccasionDto Dto { get; set; }

        public CreateOccasionCommand(CreateOccasionDto dto)
        {
            Dto = dto;
        }
    }
}
