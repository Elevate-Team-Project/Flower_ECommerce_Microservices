using MediatR;
using Microsoft.AspNetCore.Identity;
using Auth.Contarcts;
using Auth.Models;

namespace Auth.Features.Auth.UpdateUserProfile
{
    public class UpdateUserProfileHandler : IRequestHandler<UpdateUserProfileCommand, UpdateUserProfileResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ITokenService _tokenService;

        public UpdateUserProfileHandler(UserManager<ApplicationUser> userManager, ITokenService tokenService)
        {
            _userManager = userManager;
            _tokenService = tokenService;
        }

        public async Task<UpdateUserProfileResponse> Handle(UpdateUserProfileCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.UserId.ToString());
            if (user == null)
                throw new KeyNotFoundException("User not found.");

            // ✅ Update user fields if provided
            if (!string.IsNullOrWhiteSpace(request.FirstName))
                user.FirstName = request.FirstName;

            if (!string.IsNullOrWhiteSpace(request.LastName))
                user.LastName = request.LastName;

            if (!string.IsNullOrWhiteSpace(request.PhoneNumber))
                user.PhoneNumber = request.PhoneNumber;

            if (!string.IsNullOrWhiteSpace(request.ProfileImage))
                user.ProfileImageUrl = request.ProfileImage;

            user.FullName = $"{user.FirstName} {user.LastName}";

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                throw new InvalidOperationException($"Failed to update profile: {errors}");
            }

            // ✅ Generate new tokens after profile update
            // ✅ Generate new tokens after profile update
            var roles = await _userManager.GetRolesAsync(user);
            var tokens = await _tokenService.GenerateTokensAsync(user, false); // false = no rememberMe

            return new UpdateUserProfileResponse
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
        }
    }
}
