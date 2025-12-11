using MediatR;

namespace Auth.Features.Auth.ForgetPassword.OTP
{
    public record SendOtpCommand(string Email) : IRequest<bool>;

}
