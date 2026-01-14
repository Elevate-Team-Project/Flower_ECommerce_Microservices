using MediatR;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Banners.DeleteBanner
{
    public record DeleteBannerCommand(int BannerId) : IRequest<EndpointResponse<bool>>;
}
