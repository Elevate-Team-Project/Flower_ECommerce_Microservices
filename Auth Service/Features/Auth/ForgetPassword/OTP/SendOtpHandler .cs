using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Auth.Contarcts;
using Auth.Features.Auth.ForgetPassword.OTP;
using Auth.Models;

namespace Auth.Features.Auth.ForgetPassword
{
    public class SendOtpHandler : IRequestHandler<SendOtpCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;
        private readonly IMailKitEmailService _emailService;

        public SendOtpHandler(UserManager<ApplicationUser> userManager, IMemoryCache cache, IMailKitEmailService emailService)
        {
            _userManager = userManager;
            _cache = cache;
            _emailService = emailService;
        }

        public async Task<bool> Handle(SendOtpCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // Generate OTP
            var otp = new Random().Next(100000, 999999).ToString();

            // Save OTP for 5 minutes
            _cache.Set($"otp:{user.Email}", otp, TimeSpan.FromMinutes(5));

            // Send Email
            var subject = "Password Reset Code";
            string body = $@"
<!DOCTYPE html>
<html lang='en'>
<head>
<meta charset='UTF-8'>
<meta name='viewport' content='width=device-width, initial-scale=1.0'>
<style>
    body {{
        font-family: 'Segoe UI', Tahoma, Geneva, Verdana, sans-serif;
        background-color: #f9f9f9;
        color: #333;
        margin: 0;
        padding: 0;
    }}
    .email-container {{
        max-width: 600px;
        margin: 40px auto;
        background-color: #ffffff;
        border-radius: 10px;
        box-shadow: 0 5px 20px rgba(0,0,0,0.05);
        overflow: hidden;
    }}
    .header {{
        background-color: #0066ff;
        color: white;
        padding: 20px 30px;
        text-align: center;
        font-size: 24px;
        font-weight: bold;
    }}
    .content {{
        padding: 30px;
        text-align: center;
    }}
    .otp-box {{
        margin: 25px auto;
        display: inline-block;
        background-color: #f4f8ff;
        color: #0066ff;
        padding: 15px 30px;
        font-size: 32px;
        font-weight: bold;
        border-radius: 8px;
        letter-spacing: 5px;
        border: 2px solid #0066ff;
    }}
    .message {{
        font-size: 16px;
        color: #555;
        line-height: 1.6;
    }}
    .footer {{
        margin-top: 40px;
        font-size: 13px;
        color: #999;
        text-align: center;
        padding-bottom: 20px;
    }}
</style>
</head>
<body>
    <div class='email-container'>
        <div class='header'>
            Online Exam System
        </div>
        <div class='content'>
            <p class='message'>Hello,</p>
            <p class='message'>
                We received a request to verify your identity. Use the OTP code below to continue:
            </p>

            <div class='otp-box'>{otp}</div>

            <p class='message'>
                This code will expire in <strong>5 minutes</strong>. 
                If you didn’t request this code, you can safely ignore this email.
            </p>
        </div>
        <div class='footer'>
            © {DateTime.UtcNow.Year} Online Exam System. All rights reserved.
        </div>
    </div>
</body>
</html>
";
            await _emailService.SendEmailAsync(user.Email, subject, body);

            return true;
        }
    }
}
