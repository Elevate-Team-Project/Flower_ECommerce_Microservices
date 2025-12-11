namespace Auth.Features.Auth.Register
{
    public record RegisterResponse(
         bool Success,
         string Message,
         Guid UserId,
         string UserName,
         string FirstName,
         string LastName,
         string FullName,
         string PhoneNumber,
         string Email,
         string ProfileImageUrl,
            string Gender,
         IList<string> Roles ,
         string Token,
         string? RefreshToken

        );
}
