using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Auth.Models;
using Auth.Features.Auth.ForgetPassword.OTP;

namespace Auth.Features.Auth.ForgetPassword.OTP
{
    public class VerifyOtpHandler : IRequestHandler<VerifyOtpCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public VerifyOtpHandler(UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<bool> Handle(VerifyOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            var cachedOtp = _cache.Get<string>($"otp:{user.Email}");
            if (cachedOtp == null || cachedOtp != request.OtpCode)
                throw new UnauthorizedAccessException("Invalid or expired OTP.");

            // 🧠 Add flag that OTP verified successfully
            _cache.Set($"otp_verified:{user.Email}", true, TimeSpan.FromMinutes(10));

            return true;
        }
    }
}