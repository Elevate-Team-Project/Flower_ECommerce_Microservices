using MediatR;
using Microsoft.AspNetCore.Identity;
using Auth.Contarcts;
using Auth.Models;

namespace Auth.Features.Auth.Logout
{
    public class LogoutHandler : IRequestHandler<LogoutCommand, LogoutResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LogoutHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<LogoutResponse> Handle(LogoutCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            await _tokenService.RevokeRefreshTokenAsync(user);

            return new LogoutResponse
            {
                Success = true,
                Message = "Logout successful. Refresh token revoked."
            };
        }
    }
}
