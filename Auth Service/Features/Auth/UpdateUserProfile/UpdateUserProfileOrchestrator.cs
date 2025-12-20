using MediatR;
using Auth.Contarcts;

namespace Auth.Features.Auth.UpdateUserProfile
{
    public class UpdateUserProfileOrchestrator
    {
        private readonly IMediator _mediator;
        private readonly IImageHelper _imageHelper;

        public UpdateUserProfileOrchestrator(IMediator mediator, IImageHelper imageHelper)
        {
            _mediator = mediator;
            _imageHelper = imageHelper;
        }

        public async Task<UpdateUserProfileResponse> UpdateUserProfileAsync(Guid userId, UpdateUserProfileRequest request, string? currentImageUrl = null)
        {
            string? imageUrl = currentImageUrl;

            if (request.ProfileImage is not null && request.ProfileImage.Length > 0)
            {
                // Delete old image if exists
                if (!string.IsNullOrEmpty(currentImageUrl))
                {
                    _imageHelper.DeleteImage(currentImageUrl);
                }

                // Save new image
                imageUrl = await _imageHelper.SaveImageAsync(request.ProfileImage, "Users");
            }

            var command = new UpdateUserProfileCommand(
                userId,
                request.FirstName,
                request.LastName,
                request.PhoneNumber,
                imageUrl          // ProfileImage
                   
            );


            return await _mediator.Send(command);
        }
    }
}
