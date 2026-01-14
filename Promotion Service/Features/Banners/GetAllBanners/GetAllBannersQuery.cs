using MediatR;
using Promotion_Service.Features.Banners.CreateBanner;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Banners.GetAllBanners
{
    public record GetAllBannersQuery() : IRequest<EndpointResponse<List<BannerDto>>>;
}
