using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.BannersFeature.CreateBanner;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.BannersFeature.GetActiveBanners
{
    public record GetActiveBannersQuery(BannerPosition? Position = null) 
        : IRequest<EndpointResponse<List<BannerDto>>>;
}
