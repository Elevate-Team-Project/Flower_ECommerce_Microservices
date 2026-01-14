using BuildingBlocks.Interfaces;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.RegistrationCodesFeature.CreateRegistrationCode
{
    public class CreateRegistrationCodeHandler : IRequestHandler<CreateRegistrationCodeCommand, EndpointResponse<RegistrationCodeDto>>
    {
        private readonly IBaseRepository<RegistrationCode> _repository;
        private readonly IUnitOfWork _unitOfWork;

        public CreateRegistrationCodeHandler(
            IBaseRepository<RegistrationCode> repository,
            IUnitOfWork unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<EndpointResponse<RegistrationCodeDto>> Handle(
            CreateRegistrationCodeCommand request,
            CancellationToken cancellationToken)
        {
            // Check for duplicate code
            var exists = await _repository.GetAll()
                .AnyAsync(x => x.Code == request.Code, cancellationToken);

            if (exists)
                return EndpointResponse<RegistrationCodeDto>.ConflictResponse("Registration code already exists.");

            var entity = new RegistrationCode
            {
                Code = request.Code,
                Description = request.Description,
                Type = request.Type,
                TargetCustomerGroupId = request.TargetCustomerGroupId,
                WelcomeCreditAmount = request.WelcomeCreditAmount,
                Currency = request.Currency ?? "EGP",
                MaxTotalUsage = request.MaxTotalUsage,
                MaxUsagePerUser = request.MaxUsagePerUser,
                ValidFrom = request.ValidFrom,
                ValidUntil = request.ValidUntil,
                AdminNotes = request.AdminNotes,
                IsActive = request.IsActive
            };

            await _repository.AddAsync(entity);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = new RegistrationCodeDto(
                entity.Id,
                entity.Code,
                entity.Description,
                entity.Type,
                entity.WelcomeCreditAmount,
                entity.TargetCustomerGroupId,
                entity.ValidUntil,
                entity.IsActive
            );

            return EndpointResponse<RegistrationCodeDto>.SuccessResponse(dto, "Registration code created successfully.", 201);
        }
    }
}
