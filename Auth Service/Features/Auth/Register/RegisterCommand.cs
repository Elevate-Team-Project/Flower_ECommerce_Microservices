using MediatR;

namespace Auth.Features.Auth.Register
{
    public record RegisterCommand(RegisterDto RegisterDto) : IRequest<RegisterResponse>;

}
