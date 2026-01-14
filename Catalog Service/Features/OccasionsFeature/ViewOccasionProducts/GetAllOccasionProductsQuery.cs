using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.GetAllOccasions
{
    public record GetAllOccasionProductsQuery(int OccasionId) : IRequest<EndpointResponse<List<OccasionProductDto>>>;

    }
