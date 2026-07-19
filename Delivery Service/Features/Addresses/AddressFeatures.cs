using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using BuildingBlocks.Interfaces;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;

namespace Delivery_Service.Features.Addresses
{
    public static class AddressEndpoints
    {
        public static void MapAddressEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/addresses")
                .WithTags("Addresses")
                .RequireAuthorization();

            // Create Address
            group.MapPost("/", async ([FromBody] CreateAddressCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/addresses/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            }).WithName("CreateAddress");

            // Update Address
            group.MapPut("/{addressId:int}", async (int addressId, [FromBody] UpdateAddressRequest request, IMediator mediator) =>
            {
                var command = new UpdateAddressCommand(
                    addressId,
                    request.UserId,
                    request.AddressLabel,
                    request.FullName,
                    request.Phone,
                    request.Latitude,
                    request.Longitude,
                    request.Governorate,
                    request.City,
                    request.Street,
                    request.Building,
                    request.Floor,
                    request.Apartment,
                    request.PostalCode,
                    request.Country,
                    request.Notes,
                    request.Landmark
                );
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            }).WithName("UpdateAddress");

            // Delete Address
            group.MapDelete("/{addressId:int}", async (int addressId, [FromHeader(Name = "X-User-Id")] string userId, IMediator mediator) =>
            {
                var command = new DeleteAddressCommand(addressId, userId);
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            }).WithName("DeleteAddress");

            // Get User Addresses
            group.MapGet("/user/{userId}", async (string userId, IMediator mediator) =>
            {
                var query = new GetUserAddressesQuery(userId);
                var result = await mediator.Send(query);
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            }).WithName("GetUserAddresses");

            // Set Default Address
            group.MapPut("/{addressId:int}/default", async (int addressId, [FromHeader(Name = "X-User-Id")] string userId, IMediator mediator) =>
            {
                var command = new SetDefaultAddressCommand(addressId, userId);
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            }).WithName("SetDefaultAddress");
        }
    }

    // --- DTOs & MediatR Models ---

    public record CreateAddressCommand(
        string UserId,
        string AddressLabel,
        string FullName,
        string Phone,
        double? Latitude,
        double? Longitude,
        string Governorate,
        string City,
        string Street,
        string? Building,
        string? Floor,
        string? Apartment,
        string? PostalCode,
        string Country = "Egypt",
        bool IsDefault = false,
        string? Notes = null,
        string? Landmark = null
    ) : IRequest<EndpointResponse<AddressDto>>;

    public record UpdateAddressCommand(
        int AddressId,
        string UserId,
        string AddressLabel,
        string FullName,
        string Phone,
        double? Latitude,
        double? Longitude,
        string Governorate,
        string City,
        string Street,
        string? Building,
        string? Floor,
        string? Apartment,
        string? PostalCode,
        string Country,
        string? Notes,
        string? Landmark
    ) : IRequest<EndpointResponse<AddressDto>>;

    public record UpdateAddressRequest(
        string UserId,
        string AddressLabel,
        string FullName,
        string Phone,
        double? Latitude,
        double? Longitude,
        string Governorate,
        string City,
        string Street,
        string? Building,
        string? Floor,
        string? Apartment,
        string? PostalCode,
        string Country,
        string? Notes,
        string? Landmark
    );

    public record DeleteAddressCommand(int AddressId, string UserId) : IRequest<EndpointResponse<bool>>;

    public record GetUserAddressesQuery(string UserId) : IRequest<EndpointResponse<List<AddressDto>>>;

    public record SetDefaultAddressCommand(int AddressId, string UserId) : IRequest<EndpointResponse<bool>>;

    public record AddressDto(
        int Id,
        string AddressLabel,
        string FullName,
        string Phone,
        double? Latitude,
        double? Longitude,
        string Governorate,
        string City,
        string Street,
        string? Building,
        string? Floor,
        string? Apartment,
        string? PostalCode,
        string Country,
        bool IsDefault,
        string? Landmark,
        string FullAddress
    );

    // --- Handlers ---

    public class AddressHandlers :
        IRequestHandler<CreateAddressCommand, EndpointResponse<AddressDto>>,
        IRequestHandler<UpdateAddressCommand, EndpointResponse<AddressDto>>,
        IRequestHandler<DeleteAddressCommand, EndpointResponse<bool>>,
        IRequestHandler<GetUserAddressesQuery, EndpointResponse<List<AddressDto>>>,
        IRequestHandler<SetDefaultAddressCommand, EndpointResponse<bool>>
    {
        private readonly IBaseRepository<UserAddress> _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AddressHandlers(IBaseRepository<UserAddress> addressRepository, IUnitOfWork unitOfWork)
        {
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        // Handle Create
        public async Task<EndpointResponse<AddressDto>> Handle(CreateAddressCommand request, CancellationToken cancellationToken)
        {
            if (request.IsDefault)
            {
                var existingDefaults = await _addressRepository.GetAll()
                    .Where(a => a.UserId == request.UserId && a.IsDefault)
                    .ToListAsync(cancellationToken);

                foreach (var addr in existingDefaults)
                {
                    addr.IsDefault = false;
                    _addressRepository.Update(addr);
                }
            }

            var address = new UserAddress
            {
                UserId = request.UserId,
                AddressLabel = request.AddressLabel,
                FullName = request.FullName,
                Phone = request.Phone,
                Latitude = request.Latitude,
                Longitude = request.Longitude,
                Governorate = request.Governorate,
                City = request.City,
                Street = request.Street,
                Building = request.Building,
                Floor = request.Floor,
                Apartment = request.Apartment,
                PostalCode = request.PostalCode,
                Country = request.Country,
                IsDefault = request.IsDefault,
                Notes = request.Notes,
                Landmark = request.Landmark
            };

            await _addressRepository.AddAsync(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            var dto = MapToDto(address);
            return EndpointResponse<AddressDto>.SuccessResponse(dto, "Address created successfully", 201);
        }

        // Handle Update
        public async Task<EndpointResponse<AddressDto>> Handle(UpdateAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetAll()
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

            if (address == null)
                return EndpointResponse<AddressDto>.NotFoundResponse("Address not found");

            address.AddressLabel = request.AddressLabel;
            address.FullName = request.FullName;
            address.Phone = request.Phone;
            address.Latitude = request.Latitude;
            address.Longitude = request.Longitude;
            address.Governorate = request.Governorate;
            address.City = request.City;
            address.Street = request.Street;
            address.Building = request.Building;
            address.Floor = request.Floor;
            address.Apartment = request.Apartment;
            address.PostalCode = request.PostalCode;
            address.Country = request.Country;
            address.Notes = request.Notes;
            address.Landmark = request.Landmark;

            _addressRepository.Update(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<AddressDto>.SuccessResponse(MapToDto(address), "Address updated successfully");
        }

        // Handle Delete
        public async Task<EndpointResponse<bool>> Handle(DeleteAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetAll()
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

            if (address == null)
                return EndpointResponse<bool>.NotFoundResponse("Address not found");

            _addressRepository.Delete(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, "Address deleted successfully");
        }

        // Handle Get User Addresses
        public async Task<EndpointResponse<List<AddressDto>>> Handle(GetUserAddressesQuery request, CancellationToken cancellationToken)
        {
            var addresses = await _addressRepository.GetAll()
                .Where(a => a.UserId == request.UserId)
                .OrderByDescending(a => a.IsDefault)
                .ThenByDescending(a => a.CreatedAt)
                .Select(a => MapToDto(a))
                .ToListAsync(cancellationToken);

            return EndpointResponse<List<AddressDto>>.SuccessResponse(addresses, "Addresses retrieved successfully");
        }

        // Handle Set Default
        public async Task<EndpointResponse<bool>> Handle(SetDefaultAddressCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetAll()
                .FirstOrDefaultAsync(a => a.Id == request.AddressId && a.UserId == request.UserId, cancellationToken);

            if (address == null)
                return EndpointResponse<bool>.NotFoundResponse("Address not found");

            var existingDefaults = await _addressRepository.GetAll()
                .Where(a => a.UserId == request.UserId && a.IsDefault && a.Id != request.AddressId)
                .ToListAsync(cancellationToken);

            foreach (var addr in existingDefaults)
            {
                addr.IsDefault = false;
                _addressRepository.Update(addr);
            }

            address.IsDefault = true;
            _addressRepository.Update(address);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, "Default address set successfully");
        }

        private static AddressDto MapToDto(UserAddress a)
        {
            return new AddressDto(
                a.Id,
                a.AddressLabel,
                a.FullName,
                a.Phone,
                a.Latitude,
                a.Longitude,
                a.Governorate,
                a.City,
                a.Street,
                a.Building,
                a.Floor,
                a.Apartment,
                a.PostalCode,
                a.Country,
                a.IsDefault,
                a.Landmark,
                a.FullAddress
            );
        }
    }
}
