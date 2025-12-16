using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.UpdateOccasion
{
    public record UpdateOccasionCommand(int Id, UpdateOccasionDto OccasionDto) : IRequest<RequestResponse<int>>;

}
