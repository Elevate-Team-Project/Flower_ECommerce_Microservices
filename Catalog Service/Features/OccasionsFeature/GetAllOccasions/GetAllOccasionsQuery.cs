using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.GetAllOccasions
{
    public class GetAllOccasionsQuery : IRequest<EndpointResponse<List<OccasionViewModel>>>;

    }
