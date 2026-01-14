using Auth_Service.Features.Shared;
using MediatR;

public record ResetPasswordCommand(string Email, string NewPassword)
    : IRequest<RequestResponse<string>>;
