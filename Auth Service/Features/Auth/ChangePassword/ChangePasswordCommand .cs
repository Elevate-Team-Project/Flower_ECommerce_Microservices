using MediatR;

namespace Auth.Features.Auth.ChangePassword
{
    public record ChangePasswordCommand(
       string CurrentPassword,
       string NewPassword
   ) : IRequest<bool>;
}

