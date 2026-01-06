using Auth.Features.Auth.UpdateUserProfile;
using Auth_Service.Features.Shared;
using MediatR;

public record UpdateUserProfileCommand(
    Guid UserId,
    string? FirstName,
    string? LastName,
    string? PhoneNumber,
    string? ProfileImage
) : IRequest<RequestResponse<UpdateUserProfileResponse>>;
