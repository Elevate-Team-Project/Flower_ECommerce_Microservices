using Auth_Service.Features.Shared;
using MediatR;

public record VerifyOtpCommand(string Email, string OtpCode)
    : IRequest<RequestResponse<bool>>;
