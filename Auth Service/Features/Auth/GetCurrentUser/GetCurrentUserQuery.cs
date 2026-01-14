using Auth.Features.Auth.Login;
using Auth_Service.Features.Shared;
using MediatR;

namespace Auth.Features.Auth.GetCurrentUser
{
    public record GetCurrentUserQuery(string UserId)
      : IRequest<RequestResponse<LoginResponse>>;

}
