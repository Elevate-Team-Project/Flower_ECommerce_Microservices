using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using Auth.Contarcts;
using Auth.Models;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace Auth.Repositories
{
    public class JwtService : ITokenService
    {
        private readonly IConfiguration _config;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IMemoryCache _cache;

        public JwtService(IConfiguration config, UserManager<ApplicationUser> userManager, IMemoryCache cache)
        {
            _config = config;
            _userManager = userManager;
            _cache = cache;
        }

        // ========================= 🔐 Generate Tokens =========================
        public async Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user, bool rememberMe)
        {
            var roles = await GetUserRolesCachedAsync(user);

            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var accessToken = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );
            var accessTokenString = new JwtSecurityTokenHandler().WriteToken(accessToken);

            // Refresh Token
            var refreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            var refreshTokenExpiry = rememberMe ? DateTime.UtcNow.AddDays(30) : DateTime.UtcNow.AddDays(7);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = refreshTokenExpiry;
            await _userManager.UpdateAsync(user);

            return (accessTokenString, refreshToken);
        }

        // ========================= 🔁 Refresh Access Token =========================
        public async Task<string?> RefreshAccessTokenAsync(string refreshToken)
        {
            var user = await _userManager.Users
                .AsNoTracking()
                .FirstOrDefaultAsync(u => u.RefreshToken == refreshToken);

            if (user == null || user.RefreshTokenExpiryTime < DateTime.UtcNow)
                return null;

            var trackedUser = await _userManager.FindByIdAsync(user.Id.ToString());

            var roles = await GetUserRolesCachedAsync(trackedUser);

            var newAccessToken = GenerateAccessToken(trackedUser, roles);

            // Renew Refresh Token
            trackedUser.RefreshToken = Convert.ToBase64String(Guid.NewGuid().ToByteArray());
            trackedUser.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(trackedUser);

            return newAccessToken;
        }

        // ========================= 🔐 Access Token Generator =========================
        private string GenerateAccessToken(ApplicationUser user, IList<string> roles)
        {
            var claims = new List<Claim>
            {
                new Claim("id", user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };
            claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_config["JwtSettings:SecretKey"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["JwtSettings:Issuer"],
                audience: _config["JwtSettings:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(30),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ========================= ❌ Revoke Refresh Token =========================
        public async Task RevokeRefreshTokenAsync(ApplicationUser user)
        {
            user.RefreshToken = null;
            user.RefreshTokenExpiryTime = null;
            await _userManager.UpdateAsync(user);
        }

        // ========================= ⭐ Cached Roles Logic =========================
        private async Task<IList<string>> GetUserRolesCachedAsync(ApplicationUser user)
        {
            string cacheKey = $"roles_{user.Id}";

            if (_cache.TryGetValue(cacheKey, out IList<string> cachedRoles))
                return cachedRoles;

            var roles = await _userManager.GetRolesAsync(user);

            _cache.Set(
                cacheKey,
                roles,
                new MemoryCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(15),
                    Priority = CacheItemPriority.High
                }
            );

            return roles;
        }
    }
}
