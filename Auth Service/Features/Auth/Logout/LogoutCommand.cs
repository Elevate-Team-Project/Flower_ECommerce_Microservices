using MediatR;

namespace Auth.Features.Auth.Logout
{
    public record LogoutCommand(Guid UserId) : IRequest<LogoutResponse>;

}
