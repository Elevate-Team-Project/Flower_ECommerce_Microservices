using MediatR;

namespace Auth.Features.Auth.Login
{
    public record LoginCommand(string Email, string Password, bool RememberMe) : IRequest<LoginResponse>;

}
