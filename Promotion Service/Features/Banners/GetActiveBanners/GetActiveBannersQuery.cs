using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Banners.CreateBanner;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Banners.GetActiveBanners
{
    public record GetActiveBannersQuery(BannerPosition? Position = null) 
        : IRequest<EndpointResponse<List<BannerDto>>>;
}
