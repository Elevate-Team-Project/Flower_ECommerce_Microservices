using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.DeleteOccasion
{
    public record DeleteOccasionCommand(int OccasionId) : IRequest<RequestResponse<bool>>;

    
}
