using Auth_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Auth.Contarcts;
using Auth.Models;

namespace Auth.Features.Auth.ForgetPassword
{
    public class SendOtpHandler
        : IRequestHandler<SendOtpCommand, RequestResponse<string>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly IMailKitEmailService _emailService;

        public SendOtpHandler(
            UserManager<ApplicationUser> userManager,
            IMemoryCache cache,
            IMailKitEmailService emailService)
        {
            _userManager = userManager;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<RequestResponse<string>> Handle(
            SendOtpCommand request,
            CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return RequestResponse<string>.Fail("User not found.");

            var otp = new Random().Next(100000, 999999).ToString();

            _cache.Set($"otp:{user.Email}", otp, TimeSpan.FromMinutes(5));

            try
            {
                var subject = "Password Reset Code";

                string body = $"Your OTP is: {otp}";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch
            {
                return RequestResponse<string>.Fail("Failed to send email.");
            }

            return RequestResponse<string>.Success(
                otp,
                "OTP sent successfully."
            );
        }
    }
}
