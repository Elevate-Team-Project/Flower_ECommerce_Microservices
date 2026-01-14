using MediatR;
using Promotion_Service.Entities;
using Promotion_Service.Features.Shared;

namespace Promotion_Service.Features.RegistrationCodes.ValidateRegistrationCode
{
    public record ValidateRegistrationCodeCommand(
        string Code,
        string? UserId = null
    ) : IRequest<EndpointResponse<RegistrationCodeValidationResult>>;

    public record RegistrationCodeValidationResult(
        string Code,
        bool IsValid,
        string? Message,
        RegistrationCodeType Type,
        decimal? WelcomeCreditAmount,
        string? Currency,
        int? TargetCustomerGroupId
    );
}
