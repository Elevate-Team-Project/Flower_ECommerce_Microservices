using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Auth.Models;
using Auth_Service.Features.Shared;

namespace Auth.Features.Auth.ForgetPassword.ResetPassword
{
    public class ResetPasswordHandler
        : IRequestHandler<ResetPasswordCommand, RequestResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public ResetPasswordHandler(UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<RequestResponse<string>> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return RequestResponse<string>.Fail("User not found.");

            var verified = _cache.Get<bool?>($"otp_verified:{user.Email}");
            if (verified is null or false)
                return RequestResponse<string>.Fail("OTP not verified.");

            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return RequestResponse<string>.Fail(errors);
            }

            _cache.Remove($"otp:{user.Email}");
            _cache.Remove($"otp_verified:{user.Email}");

            return RequestResponse<string>.Success(
                "Password reset successfully",
                "Password reset successfully"
            );
        }
    }
}
