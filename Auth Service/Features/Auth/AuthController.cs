using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Auth.Features.Auth.ChangePassword;
using Auth.Features.Auth.ForgetPassword.OTP;
using Auth.Features.Auth.ForgetPassword.ResetPassword;
using Auth.Features.Auth.GetCurrentUser;
using Auth.Features.Auth.Login;
using Auth.Features.Auth.Logout;
using Auth.Features.Auth.Register;
using Auth.Features.Auth.UpdateUserProfile;

namespace Auth.Features.Auth
{
    [Route("api/[controller]")]
    [ApiController]

    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly UpdateUserProfileOrchestrator _updateUserProfileOrchestrator;  
        public AuthController(IMediator mediator , UpdateUserProfileOrchestrator updateUserProfileOrchestrator) { 
        
            _mediator = mediator;
            _updateUserProfileOrchestrator = updateUserProfileOrchestrator;


        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            var command = new RegisterCommand(dto);
            var result = await _mediator.Send(command);

            if (!result.Success)
                return BadRequest(result);

            return Ok(result);
        }


        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(result);
        }

        [HttpGet("user-info")]
        [Authorize]
        public async Task<IActionResult> GetCurrentUser()
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { statusCode = 401, message = "Invalid token or not authenticated." });

            var result = await _mediator.Send(new GetCurrentUserQuery(userId));
            return Ok(result);
        }

        [HttpPost("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "Password changed successfully." });
        }

        [HttpPut("update-profile")]
        [Authorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateUserProfileRequest request)
        {
            var userId = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new { statusCode = 401, message = "Invalid token or not authenticated." });

            var response = await _updateUserProfileOrchestrator.UpdateUserProfileAsync(
                Guid.Parse(userId),
                request
            );

            return Ok(response);
        }

        [HttpPost("forget-password")]
        public async Task<IActionResult> ForgetPassword([FromBody] SendOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "OTP sent successfully to your email." });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "OTP verified successfully." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordCommand command)
        {
            var result = await _mediator.Send(command);
            return Ok(new { success = result, message = "Password reset successfully." });
        }



        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            var userIdClaim = User.FindFirst("id")?.Value;
            if (string.IsNullOrEmpty(userIdClaim))
                return Unauthorized(new { message = "Invalid token." });

            var userId = Guid.Parse(userIdClaim);
            var result = await _mediator.Send(new LogoutCommand(userId));

            return Ok(result);
        }


    }
}
