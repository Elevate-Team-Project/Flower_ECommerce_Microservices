using Auth.Models;

namespace Auth.Contarcts
{
    public interface ITokenService
    {
        /// <summary>
        /// Generates a new Access & Refresh token pair for the specified user.
        /// </summary>
        /// <param name="user">The user to generate tokens for.</param>
        /// <param name="rememberMe">Determines whether to extend refresh token lifetime.</param>
        /// <returns>Tuple of (AccessToken, RefreshToken)</returns>
        Task<(string AccessToken, string RefreshToken)> GenerateTokensAsync(ApplicationUser user, bool rememberMe);

        /// <summary>
        /// Uses a valid refresh token to generate a new access token.
        /// </summary>
        /// <param name="refreshToken">Existing refresh token.</param>
        /// <returns>New access token if refresh token is valid, otherwise null.</returns>
        Task<string?> RefreshAccessTokenAsync(string refreshToken);

        /// <summary>
        /// Revokes user's refresh token (used on logout).
        /// </summary>
        /// <param name="user">User to revoke refresh token for.</param>
        Task RevokeRefreshTokenAsync(ApplicationUser user);
    }
}
