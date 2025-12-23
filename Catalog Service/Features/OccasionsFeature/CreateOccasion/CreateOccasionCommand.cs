using Catalog_Service.Features.Shared;
using Humanizer;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.CreateOccasion
{
    public class CreateOccasionCommand : IRequest<RequestResponse<CreateOccasionDto>>
    {
        public CreateOccasionDto Dto { get; set; }

        public CreateOccasionCommand(CreateOccasionDto dto)
        {
            Dto = dto;
        }
    }
}
