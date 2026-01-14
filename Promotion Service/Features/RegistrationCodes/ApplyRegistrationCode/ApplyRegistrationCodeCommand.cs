using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.RegistrationCodes.ApplyRegistrationCode
{
    public record ApplyRegistrationCodeCommand(
        string Code,
        string UserId,
        string? IpAddress = null
    ) : IRequest<EndpointResponse<ApplyRegistrationCodeResult>>;

    public record ApplyRegistrationCodeResult(
        bool Success,
        string Message,
        RegistrationCodeType Type,
        decimal? CreditAmount,
        string? Currency,
        int? TargetGroupId
    );
}
