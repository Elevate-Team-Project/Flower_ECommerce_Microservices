using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.RegistrationCodes.CreateRegistrationCode
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
