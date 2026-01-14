using Auth_Service.Features.Shared;
using MediatR;

namespace Auth.Features.Auth.ChangePassword
{
    public record ChangePasswordCommand(
        string CurrentPassword,
        string NewPassword
    ) : IRequest<RequestResponse<ChangePasswordResponse>>;
}
