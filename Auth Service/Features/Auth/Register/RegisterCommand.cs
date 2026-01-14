using Auth_Service.Features.Shared;
using MediatR;

namespace Auth.Features.Auth.Register
{
    public record RegisterCommand(RegisterDto RegisterDto)
         : IRequest<RequestResponse<RegisterResponse>>;

}
