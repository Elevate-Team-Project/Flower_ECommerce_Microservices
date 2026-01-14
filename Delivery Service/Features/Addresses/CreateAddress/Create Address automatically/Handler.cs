using BuildingBlocks.Interfaces;
using Delivery_Service.Entities;
using Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically.DTOs;
using Delivery_Service.Features.Shared;
using Delivery_Service.Infrastructure.Services;
using MediatR;

namespace Delivery_Service.Features.Addresses.CreateAddress.Create_Address_automatically
{
    public class Handler : IRequestHandler<CreateAddressAutomaticallyCommand, RequestResponse<AddressResponse>>
    {
        private readonly IBaseRepository<DeliveryAddress> _addressRepository;
        private readonly IGeocodingService _geocodingService;
        private readonly IUnitOfWork _unitOfWork;

        public Handler(
            IBaseRepository<DeliveryAddress> addressRepository,
            IGeocodingService geocodingService,
            IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _geocodingService = geocodingService;
            _unitOfWork = unitOfWork;
        }

        public async Task<RequestResponse<AddressResponse>> Handle(CreateAddressAutomaticallyCommand request, CancellationToken cancellationToken)
        {
            var dto = request.CreateAddressRequest;

            // Coordinates come directly from the map selection on the frontend
            if (dto.Latitude is < -90 or > 90 || dto.Longitude is < -180 or > 180)
                return RequestResponse<AddressResponse>.Fail("Invalid latitude/longitude values.");

            // Reverse geocode: get formatted address from coordinates (lat/long -> address)
            var addressAutoDto = await _geocodingService.GetAddressFromCoordinates(dto.Latitude, dto.Longitude);

           
            var address = new DeliveryAddress
            {
                UserId = dto.UserId.ToString(),
                Label = dto.Label,
              city= addressAutoDto.City ?? "Unknown City",
                country= addressAutoDto.Country ?? "Unknown Country",
                Street=addressAutoDto.Street ?? "Unknown Street",
                fullAddress = $"{addressAutoDto.Street}, {addressAutoDto.City}, {addressAutoDto.Country}",
                Building= null,
                Floor= null,
                postalCode= addressAutoDto.PostalCode,
                phone=request.CreateAddressRequest.phonenumer,
                
                Latitude = dto.Latitude,
                Longitude = dto.Longitude,
                CreatedAt = DateTime.UtcNow
            };

            await _addressRepository.AddAsync(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var response = new AddressResponse
            {
                Id = address.Id,
                Label = address.Label,
                postalCode = address.postalCode ?? "",
                FormattedAddress = address.fullAddress,
                Location = new LocationDto
                {
                    Latitude = address.Latitude,
                    Longitude = address.Longitude
                }
            };

            return RequestResponse<AddressResponse>.Success(response, "Address created automatically.");
        }
    }
}
