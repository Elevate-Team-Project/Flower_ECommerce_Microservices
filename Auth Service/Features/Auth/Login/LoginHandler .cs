using Auth.Contarcts;
using Auth.Models;
using Auth_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;

namespace Auth.Features.Auth.Login
{
    public class LoginHandler
       : IRequestHandler<LoginCommand, RequestResponse<LoginResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public LoginHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<RequestResponse<LoginResponse>> Handle(LoginCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByEmailAsync(request.Email);

            if (user == null)
                return RequestResponse<LoginResponse>.Fail("Invalid email or password.");

            var isPasswordValid = await _userManager.CheckPasswordAsync(user, request.Password);

            if (!isPasswordValid)
                return RequestResponse<LoginResponse>.Fail("Invalid email or password.");

            var (accessToken, refreshToken) =
                await _tokenService.GenerateTokensAsync(user, request.RememberMe);

            var roles = await _userManager.GetRolesAsync(user);

            var result = new LoginResponse
            {
                Success = true,
                Message = "Login successful",
                UserId = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                lastName = user.LastName,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                Gender = user.Gender,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = roles,
                Token = accessToken,
                RefreshToken = refreshToken
            };

            return RequestResponse<LoginResponse>.Success(result, "Login successful");
        }
    }
}
