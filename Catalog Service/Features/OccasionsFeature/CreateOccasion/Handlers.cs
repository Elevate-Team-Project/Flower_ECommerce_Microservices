using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.CreateOccasion
{
    public class Handlers : IRequestHandler<CreateOccasionCommand, CreateOccasionDto>
    {
        private readonly IBaseRepository<Occasion> _occasionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public Handlers(IBaseRepository<Occasion> occasionRepo, IUnitOfWork unitOfWork)
        {
            _occasionRepo = occasionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<CreateOccasionDto> Handle(CreateOccasionCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var exists = _occasionRepo.Get(o => o.Name == dto.Name).Any();
            if (exists)
                throw new Exception("Occasion name already exists.");

            var occasion = new Occasion
            {
                Name = dto.Name,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _occasionRepo.AddAsync(occasion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return new CreateOccasionDto
            {
                Name = occasion.Name,
                IsActive = occasion.IsActive
            };
        }
    }
}
