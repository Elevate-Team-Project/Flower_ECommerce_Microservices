using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MediatR;
using System.Security.Permissions;

namespace Catalog_Service.Features.OccasionsFeature.UpdateOccasion
{
   public class UpdateOccasionCommandHandler:IRequestHandler<UpdateOccasionCommand,int>

    {
        private readonly IBaseRepository<Occasion> _repo;
        private readonly IUnitOfWork _unitOfWork;
        public UpdateOccasionCommandHandler(IBaseRepository<Occasion> repo,IUnitOfWork unitOfWork)
        {
            _repo = repo;
            _unitOfWork = unitOfWork;
        }
        
        public async Task<int>Handle(UpdateOccasionCommand request,CancellationToken cancellationToken)
        {
            var dto = request.OccasionDto;
                        var occasion = await _repo.GetByIdAsync(request.Id);
            if (occasion == null)
                throw new Exception("Occasion not found");

            occasion.Name = dto.Name;
                occasion.Description = dto.Description;
                occasion.ImageUrl = dto.ImageUrl;
            _repo.SaveInclude(occasion);
            await _unitOfWork.SaveChangesAsync();

            return occasion.Id;
            }
    }
}
