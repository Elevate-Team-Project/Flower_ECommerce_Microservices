using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.RegistrationCodesFeature.ApplyRegistrationCode
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
