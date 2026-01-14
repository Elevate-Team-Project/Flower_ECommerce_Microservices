using Auth.Contarcts;
using Auth.Data;
using Auth.Models;
using Auth_Service.Features.Shared;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Auth.Features.Auth.Register
{

    public class RegisterHandler
          : IRequestHandler<RegisterCommand, RequestResponse<RegisterResponse>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly ITokenService _tokenService;
        private readonly IMemoryCache _cache;
        private readonly FlowerEcommerceAuthContext _db;

        public RegisterHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            ITokenService tokenService,
            IMemoryCache cache,
            FlowerEcommerceAuthContext dbContext
        )
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _tokenService = tokenService;
            _cache = cache;
            _db = dbContext;
        }

        public async Task<RequestResponse<RegisterResponse>> Handle(RegisterCommand request, CancellationToken cancellationToken)
        {
            var dto = request.RegisterDto;

            using var transaction = await _db.Database.BeginTransactionAsync(cancellationToken);

            var existingUser = await _userManager.FindByEmailAsync(dto.Email);
            if (existingUser != null)
                return RequestResponse<RegisterResponse>.Fail("Email already registered.");

            var user = new ApplicationUser
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                FullName = $"{dto.FirstName} {dto.LastName}",
                Email = dto.Email,
                UserName = dto.Email,
                PhoneNumber = dto.PhoneNumber,
                Gender = dto.Gender,
                EmailConfirmed = true,
                ProfileImageUrl = "/images/users/default-user.png",
                CreatedAt = DateTime.UtcNow
            };

            var result = await _userManager.CreateAsync(user, dto.Password);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                return RequestResponse<RegisterResponse>.Fail($"User creation failed: {errors}");
            }

            string userRoleKey = "role_exists_user";

            bool roleExists = await _cache.GetOrCreateAsync(userRoleKey, async entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15);
                return await _roleManager.RoleExistsAsync("User");
            });

            if (roleExists)
                await _userManager.AddToRoleAsync(user, "User");

            await transaction.CommitAsync();

            var roles = await _userManager.GetRolesAsync(user);

            var (accessToken, refreshToken) =
                await _tokenService.GenerateTokensAsync(user, dto.RememberMe);

            var response = new RegisterResponse(
                Success: true,
                Message: "Registration successful",
                UserId: user.Id,
                UserName: user.UserName,
                FirstName: user.FirstName,
                LastName: user.LastName,
                FullName: user.FullName,
                Email: user.Email,
                Gender: user.Gender,
                PhoneNumber: user.PhoneNumber,
                ProfileImageUrl: user.ProfileImageUrl,
                Roles: roles,
                Token: accessToken,
                RefreshToken: refreshToken
            );

            return RequestResponse<RegisterResponse>.Success(response, "Registration successful");
        }
    }
}
