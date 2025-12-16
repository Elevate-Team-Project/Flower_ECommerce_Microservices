using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;
using System.Security.Permissions;

namespace Catalog_Service.Features.OccasionsFeature.UpdateOccasion
{
   public class UpdateOccasionCommandHandler:IRequestHandler<UpdateOccasionCommand, RequestResponse<int>>

    {
        private readonly IBaseRepository<Occasion> _repo;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateOccasionCommandHandler(IBaseRepository<Occasion> repo,IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<RequestResponse<int>> Handle(UpdateOccasionCommand request,CancellationToken cancellationToken)
        {
            var dto = request.OccasionDto;
                        var occasion = await _repo.GetByIdAsync(request.Id);
            if (occasion == null)
                return RequestResponse<int>.Fail("Occasion not found");

            occasion.Name = dto.Name;
                occasion.Description = dto.Description;
                occasion.ImageUrl = dto.ImageUrl;
            _repo.SaveInclude(occasion);
            await _unitOfWork.SaveChangesAsync();


            return RequestResponse<int>.Success(occasion.Id, "Updated successfully");
        }
    }
}
