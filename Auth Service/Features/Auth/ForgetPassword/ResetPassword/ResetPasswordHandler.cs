using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Auth.Models;
using Auth.Features.Auth.ForgetPassword.ResetPassword;

namespace Auth.Features.Auth.ForgetPassword.ResetPassword
{

    public class ResetPasswordHandler : IRequestHandler<ResetPasswordCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public ResetPasswordHandler(UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<bool> Handle(ResetPasswordCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // ✅ Check OTP verified
            var verified = _cache.Get<bool?>($"otp_verified:{user.Email}");
            if (verified is null or false)
                throw new UnauthorizedAccessException("OTP not verified.");

            // 🔐 Reset password
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var result = await _userManager.ResetPasswordAsync(user, token, request.NewPassword);

            if (!result.Succeeded)
                throw new InvalidOperationException(string.Join("; ", result.Errors.Select(e => e.Description)));

            // 🧹 Clean cache
            _cache.Remove($"otp:{user.Email}");
            _cache.Remove($"otp_verified:{user.Email}");

            return true;
        }
    }
}