using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.RegistrationCodesFeature.CreateRegistrationCode
{
    public record CreateRegistrationCodeCommand(
        string Code,
        string? Description,
        RegistrationCodeType Type,
        int? TargetCustomerGroupId,
        decimal? WelcomeCreditAmount,
        string Currency,
        int? MaxTotalUsage,
        int? MaxUsagePerUser,
        DateTime ValidFrom,
        DateTime ValidUntil,
        string? AdminNotes,
        bool IsActive
    ) : IRequest<EndpointResponse<RegistrationCodeDto>>;

    public record RegistrationCodeDto(
        int Id,
        string Code,
        string? Description,
        RegistrationCodeType Type,
        decimal? WelcomeCreditAmount,
        int? TargetCustomerGroupId,
        DateTime ValidUntil,
        bool IsActive
    );
}
