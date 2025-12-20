using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Auth.Features.Auth.GetCurrentUser;
using Auth.Models;

namespace Auth.Features.Auth.Login
{
    // ⚡ Handler for GetCurrentUserQuery with Memory Caching
    public class GetCurrentUserHandler : IRequestHandler<GetCurrentUserQuery, LoginResponse>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public GetCurrentUserHandler(UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _userManager = userManager;
            _cache = cache;
        }

        public async Task<LoginResponse> Handle(GetCurrentUserQuery request, CancellationToken cancellationToken)
        {
            string cacheKey = $"current_user_{request.UserId}";

            // ⚡ Check if cached
            if (_cache.TryGetValue(cacheKey, out LoginResponse cachedUser))
            {
                return cachedUser;
            }

            var user = await _userManager.FindByIdAsync(request.UserId);
            if (user == null)
                throw new KeyNotFoundException("User not found.");

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

                Roles = roles,
            };

            // 🧠 Save to memory cache for 5 minutes
            var cacheOptions = new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            };

            _cache.Set(cacheKey, response, cacheOptions);

            return response;
        }
    }
}
