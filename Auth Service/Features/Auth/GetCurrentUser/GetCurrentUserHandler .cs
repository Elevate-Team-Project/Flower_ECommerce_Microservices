using Auth.Features.Auth.GetCurrentUser;
using Auth.Models;
using Auth_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;

namespace Auth.Features.Auth.Login
{
    // ⚡ Handler for GetCurrentUserQuery with Memory Caching
    public class GetCurrentUserHandler
            : IRequestHandler<GetCurrentUserQuery, RequestResponse<LoginResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public GetCurrentUserHandler(UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<RequestResponse<LoginResponse>> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"current_user_{request.UserId}";

            if (_cache.TryGetValue(cacheKey, out LoginResponse cachedUser))
            {
                return RequestResponse<LoginResponse>.Success(
                    cachedUser,
                    "User retrieved successfully (cached)"
                );
            }

            var user = await _userManager.FindByIdAsync(request.UserId);

            if (user == null)
                return RequestResponse<LoginResponse>.Fail("User not found.");

            var roles = await _userManager.GetRolesAsync(user);

            var response = new LoginResponse
            {
                Success = true,
                Message = "User retrieved successfully",
                UserId = user.Id,
                UserName = user.UserName,
                FirstName = user.FirstName,
                lastName = user.LastName,
                PhoneNumber = user.PhoneNumber,
                FullName = $"{user.FirstName} {user.LastName}",
                Email = user.Email,
                ProfileImageUrl = user.ProfileImageUrl,
                Roles = roles
            };

            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, cacheOptions);

            return RequestResponse<LoginResponse>.Success(
                response,
                "User retrieved successfully"
            );
        }
    }
}
