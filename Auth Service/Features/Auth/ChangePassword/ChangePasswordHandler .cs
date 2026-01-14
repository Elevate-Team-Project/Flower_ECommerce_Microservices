using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Auth.Models;
using MediatR;
using Auth_Service.Features.Shared;

namespace Auth.Features.Auth.ChangePassword
{
    public class ChangePasswordHandler
        : IRequestHandler<ChangePasswordCommand, RequestResponse<ChangePasswordResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public ChangePasswordHandler(UserManager<ApplicationUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<RequestResponse<ChangePasswordResponse>> Handle(ChangePasswordCommand request, CancellationToken cancellationToken)
        {
            var userId = _httpContextAccessor.HttpContext?.User.FindFirst("id")?.Value;

            if (string.IsNullOrEmpty(userId))
                return RequestResponse<ChangePasswordResponse>.Fail("Invalid token or not authenticated.");

            var user = await _userManager.FindByIdAsync(userId);

            if (user == null)
                return RequestResponse<ChangePasswordResponse>.Fail("User not found.");

            var result = await _userManager.ChangePasswordAsync(
                user,
                request.CurrentPassword,
                request.NewPassword
            );

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return RequestResponse<ChangePasswordResponse>.Fail($"Failed to change password: {errors}");
            }

            var response = new ChangePasswordResponse(
                Success: true,
                Message: "Password changed successfully."
            );

            return RequestResponse<ChangePasswordResponse>.Success(
                response,
                "Password changed successfully."
            );
        }
    }
}
