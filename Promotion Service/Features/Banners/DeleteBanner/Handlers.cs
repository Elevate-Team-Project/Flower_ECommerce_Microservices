using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.Banners.DeleteBanner
{
    public class DeleteBannerHandler : IRequestHandler<DeleteBannerCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<Banner> _bannerRepository;
        private readonly IUnitOfWork _unitOfWork;

        public DeleteBannerHandler(
            IBaseRepository<Banner> bannerRepository,
            IUnitOfWork unitOfWork)
        {
            _bannerRepository = bannerRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<bool>> Handle(
            DeleteBannerCommand request,
            CancellationToken cancellationToken)
        {
            var banner = await _bannerRepository.GetAll()
                .FirstOrDefaultAsync(b => b.Id == request.BannerId, cancellationToken);

            if (banner == null)
                return EndpointResponse<bool>.NotFoundResponse("Banner not found");

            _bannerRepository.Delete(banner);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, "Banner deleted successfully");
        }
    }
}
