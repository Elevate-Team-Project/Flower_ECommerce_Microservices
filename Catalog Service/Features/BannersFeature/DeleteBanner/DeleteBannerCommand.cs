using MediatR;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.BannersFeature.DeleteBanner
{
    public record DeleteBannerCommand(int BannerId) : IRequest<EndpointResponse<bool>>;
}
