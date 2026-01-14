using Auth_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Auth.Models;

namespace Auth.Features.Auth.ForgetPassword.OTP
{
    public class VerifyOtpHandler
        : IRequestHandler<VerifyOtpCommand, RequestResponse<bool>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public VerifyOtpHandler(
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<RequestResponse<bool>> Handle(
            VerifyOtpCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return RequestResponse<bool>.Fail("User not found.");

            var cachedOtp = _cache.Get<string>($"otp:{user.Email}");

            if (cachedOtp == null || cachedOtp != request.OtpCode)
                return RequestResponse<bool>.Fail("Invalid or expired OTP.");

            _cache.Set($"otp_verified:{user.Email}", true, TimeSpan.FromMinutes(10));

            return RequestResponse<bool>.Success(true, "OTP verified successfully.");
        }
    }
}
