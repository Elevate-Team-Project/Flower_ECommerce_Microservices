namespace Auth.Features.Auth.UpdateUserProfile
{
    public record UpdateUserProfileResponse
    {
        public bool Success { get; init; }
        public string Message { get; init; }
        public Guid UserId { get; init; }
        public string UserName { get; init; }
        public string FirstName { get; init; }
        public string lastName { get; init; }
        public string FullName { get; init; }
        public string Email { get; init; }
        public string PhoneNumber { get; init; }
        public string? ProfileImageUrl { get; init; }
        public IEnumerable<string>? Roles { get; init; }
        public string Token { get; init; }
      
        public string RefreshToken { get; init; }
    }

}
