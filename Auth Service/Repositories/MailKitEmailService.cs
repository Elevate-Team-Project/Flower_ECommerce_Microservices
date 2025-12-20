using MailKit.Net.Smtp;
using MimeKit;
using Auth.Contarcts;
using Microsoft.Extensions.Configuration;

namespace Auth.Repositories
{
    public class MailKitEmailService : IMailKitEmailService
    {
        private readonly IConfiguration _config;

        public MailKitEmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpEmailAsync(string toEmail, string otpCode)
        {
            string subject = "Your OTP Code - Online Exam System";

            // Build HTML body
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

            <div class='otp-box'>{otpCode}</div>

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

            await SendEmailAsync(toEmail, subject, body);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var email = new MimeMessage();
            email.From.Add(MailboxAddress.Parse(_config["MailSettings:From"]));
            email.To.Add(MailboxAddress.Parse(toEmail));
            email.Subject = subject;
            email.Body = new TextPart(MimeKit.Text.TextFormat.Html) { Text = body };

            using var smtp = new SmtpClient();
            await smtp.ConnectAsync(
                _config["MailSettings:Host"],
                int.Parse(_config["MailSettings:Port"]),
                false
            );
            await smtp.AuthenticateAsync(_config["MailSettings:Username"], _config["MailSettings:Password"]);
            await smtp.SendAsync(email);
            await smtp.DisconnectAsync(true);
        }
    }
}
