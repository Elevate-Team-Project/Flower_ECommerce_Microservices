using MediatR;
using Catalog_Service.Entities;
using Catalog_Service.Features.Shared;

namespace Catalog_Service.Features.RegistrationCodesFeature.ValidateRegistrationCode
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
