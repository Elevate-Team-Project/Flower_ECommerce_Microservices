using MediatR;

namespace Auth.Features.Auth.ForgetPassword.OTP
{
    public record VerifyOtpCommand(string Email, string OtpCode) : IRequest<bool>;

}
