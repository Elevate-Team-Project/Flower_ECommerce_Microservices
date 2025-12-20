using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.UpdateOccasion
{
    public record ActivateDeactivateOcassionComand(int Id, ActivateDeactivateOcassionDto ADOccasionDto) : IRequest<RequestResponse<int>>;

}
