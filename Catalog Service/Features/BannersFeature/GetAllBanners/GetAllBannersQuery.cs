using MediatR;
using Catalog_Service.Features.BannersFeature.CreateBanner;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.BannersFeature.GetAllBanners
{
    public record GetAllBannersQuery() : IRequest<EndpointResponse<List<BannerDto>>>;
}
