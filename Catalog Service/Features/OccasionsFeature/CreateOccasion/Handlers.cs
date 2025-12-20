using BuildingBlocks.Interfaces;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;
using MediatR;

namespace Catalog_Service.Features.OccasionsFeature.CreateOccasion
{
    public class Handlers : IRequestHandler<CreateOccasionCommand, RequestResponse<CreateOccasionDto>>
    {
        private readonly IBaseRepository<Occasion> _occasionRepo;
        private readonly IUnitOfWork _unitOfWork;

        public Handlers(IBaseRepository<Occasion> occasionRepo, IUnitOfWork unitOfWork)
        {
            _occasionRepo = occasionRepo;
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<CreateOccasionDto>> Handle(CreateOccasionCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var exists = _occasionRepo.Get(o => o.Name == dto.Name).Any();
            if (exists)
                return RequestResponse<CreateOccasionDto>.Fail("Occasion name already exists.");

            var occasion = new Occasion
            {
                Name = dto.Name,
                IsActive = dto.IsActive,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _occasionRepo.AddAsync(occasion);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return RequestResponse<CreateOccasionDto>.Success(
                new CreateOccasionDto
                {
                    Name = occasion.Name,
                    IsActive = occasion.IsActive
                },
                "Occasion created successfully"
            );
        }
    }
}
