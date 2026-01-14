using Auth_Service.Features.Shared;
using MediatR;

public record SendOtpCommand(string Email)
    : IRequest<RequestResponse<string>>;
