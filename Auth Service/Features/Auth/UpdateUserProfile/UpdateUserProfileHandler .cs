using MediatR;
using Microsoft.AspNetCore.Identity;
using Auth.Contarcts;
using Auth.Models;
using Auth_Service.Features.Shared;

namespace Auth.Features.Auth.UpdateUserProfile
{
    public class UpdateUserProfileHandler
        : IRequestHandler<UpdateUserProfileCommand, RequestResponse<UpdateUserProfileResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public UpdateUserProfileHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<RequestResponse<UpdateUserProfileResponse>> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());

            if (user == null)
                return RequestResponse<UpdateUserProfileResponse>.Fail("User not found.");

            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.ProfileImage))
                user.ProfileImageUrl = request.ProfileImage;

            user.FullName = $"{user.FirstName} {user.LastName}";

            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                var errors = string.Join("; ", result.Errors.Select(e => e.Description));
                return RequestResponse<UpdateUserProfileResponse>.Fail($"Failed to update profile: {errors}");
            }

            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await _tokenService.GenerateTokensAsync(user, false);

            var response = new UpdateUserProfileResponse
            {
                Success = true,
                Message = "Profile updated successfully",
                UserId = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                lastName = user.LastName,
                FullName = user.FullName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = roles,
                Token = tokens.AccessToken,
                RefreshToken = tokens.RefreshToken
            };

            return RequestResponse<UpdateUserProfileResponse>.Success(
                response,
                "Profile updated successfully"
            );
        }
    }
}
