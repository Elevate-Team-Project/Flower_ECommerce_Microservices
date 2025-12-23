using MediatR;
using Auth.Features.Auth.Login;

namespace Auth.Features.Auth.GetCurrentUser
{
    public record GetCurrentUserQuery(string UserId) : IRequest<LoginResponse>;

}
