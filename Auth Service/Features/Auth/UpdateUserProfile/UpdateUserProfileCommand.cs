using Auth.Features.Auth.UpdateUserProfile;
using MediatR;

public record UpdateUserProfileCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? ProfileImage
 
) : IRequest<UpdateUserProfileResponse>;
