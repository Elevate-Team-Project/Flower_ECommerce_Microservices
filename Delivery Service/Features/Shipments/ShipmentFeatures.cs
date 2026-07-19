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
using BuildingBlocks.IntegrationEvents;
using Delivery_Service.Entities;
using Delivery_Service.Features.Shared;
using MassTransit;

namespace Delivery_Service.Features.Shipments
{
    public static class ShipmentEndpoints
    {
        public static void MapShipmentEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/shipments")
                .WithTags("Shipments")
                .RequireAuthorization();

            // Create Shipment manually
            group.MapPost("/", async ([FromBody] CreateShipmentCommand command, IMediator mediator) =>
            {
                var result = await mediator.Send(command);
                return result.IsSuccess
                    ? Results.Created($"/api/shipments/{result.Data?.Id}", result)
                    : Results.BadRequest(result);
            }).WithName("CreateShipment");

            // Get Shipment Details by Tracking Number
            group.MapGet("/tracking/{trackingNumber}", async (string trackingNumber, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetShipmentByTrackingQuery(trackingNumber));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            }).WithName("GetShipmentByTracking");

            // Update Driver Location
            group.MapPut("/{shipmentId:int}/driver-location", async (int shipmentId, [FromBody] UpdateDriverLocationRequest request, IMediator mediator) =>
            {
                var command = new UpdateDriverLocationCommand(
                    shipmentId,
                    request.Latitude,
                    request.Longitude,
                    request.DriverName,
                    request.DriverPhone
                );
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
            }).WithName("UpdateDriverLocation");

            // Update Shipment Status
            group.MapPut("/{shipmentId:int}/status", async (int shipmentId, [FromBody] UpdateShipmentStatusRequest request, IMediator mediator) =>
            {
                var command = new UpdateShipmentStatusCommand(
                    shipmentId,
                    request.NewStatus,
                    request.CurrentLocation,
                    request.Notes
                );
                var result = await mediator.Send(command);
                return result.IsSuccess ? Results.Ok(result) : Results.BadRequest(result);
            }).WithName("UpdateShipmentStatus");

            // Get Delivery Tracking Info for Map Display
            group.MapGet("/{shipmentId:int}/tracking", async (int shipmentId, IMediator mediator) =>
            {
                var result = await mediator.Send(new GetDeliveryTrackingQuery(shipmentId));
                return result.IsSuccess ? Results.Ok(result) : Results.NotFound(result);
            }).WithName("GetDeliveryTracking");
        }
    }

    // --- MediatR Models ---

    public record CreateShipmentCommand(
        int OrderId,
        int DeliveryAddressId,
        string TrackingNumber,
        string Carrier,
        DateTime? EstimatedDeliveryDate,
        bool IsGift = false,
        string? RecipientName = null,
        string? RecipientPhone = null,
        string? GiftMessage = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<ShipmentDto>>;

    public record GetShipmentByTrackingQuery(string TrackingNumber) : IRequest<EndpointResponse<ShipmentDto>>;

    public record UpdateDriverLocationCommand(
        int ShipmentId,
        double Latitude,
        double Longitude,
        string? DriverName = null,
        string? DriverPhone = null
    ) : IRequest<EndpointResponse<bool>>;

    public record UpdateDriverLocationRequest(
        double Latitude,
        double Longitude,
        string? DriverName = null,
        string? DriverPhone = null
    );

    public record UpdateShipmentStatusCommand(
        int ShipmentId,
        string NewStatus,
        string? CurrentLocation = null,
        string? Notes = null
    ) : IRequest<EndpointResponse<bool>>;

    public record UpdateShipmentStatusRequest(
        string NewStatus,
        string? CurrentLocation = null,
        string? Notes = null
    );

    public record GetDeliveryTrackingQuery(int ShipmentId) : IRequest<EndpointResponse<DeliveryTrackingDto>>;

    public record ShipmentDto(
        int Id,
        int OrderId,
        int DeliveryAddressId,
        string TrackingNumber,
        string Carrier,
        string Status,
        DateTime? EstimatedDeliveryDate,
        DateTime? ActualDeliveryDate,
        string? CurrentLocation,
        bool IsGift,
        string? RecipientName,
        string? GiftMessage,
        DateTime CreatedAt
    );

    public record DeliveryTrackingDto(
        int ShipmentId,
        string TrackingNumber,
        string Carrier,
        string Status,
        double? DriverLatitude,
        double? DriverLongitude,
        DateTime? LastLocationUpdate,
        string? DriverName,
        string? DriverPhone,
        string? DriverPhotoUrl
    );

    // --- MediatR Handlers ---

    public class ShipmentHandlers :
        IRequestHandler<CreateShipmentCommand, EndpointResponse<ShipmentDto>>,
        IRequestHandler<GetShipmentByTrackingQuery, EndpointResponse<ShipmentDto>>,
        IRequestHandler<UpdateDriverLocationCommand, EndpointResponse<bool>>,
        IRequestHandler<UpdateShipmentStatusCommand, EndpointResponse<bool>>,
        IRequestHandler<GetDeliveryTrackingQuery, EndpointResponse<DeliveryTrackingDto>>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IBaseRepository<UserAddress> _addressRepository;
        private readonly IUnitOfWork _unitOfWork;

        public ShipmentHandlers(
            IBaseRepository<Shipment> shipmentRepository,
            IBaseRepository<UserAddress> addressRepository,
            IUnitOfWork unitOfWork)
        {
            _shipmentRepository = shipmentRepository;
            _addressRepository = addressRepository;
            _unitOfWork = unitOfWork;
        }

        // Handle Create Shipment
        public async Task<EndpointResponse<ShipmentDto>> Handle(CreateShipmentCommand request, CancellationToken cancellationToken)
        {
            var address = await _addressRepository.GetByIdAsync(request.DeliveryAddressId);
            if (address == null)
                return EndpointResponse<ShipmentDto>.ErrorResponse($"Delivery address {request.DeliveryAddressId} not found", 404);

            var shipment = new Shipment
            {
                OrderId = request.OrderId,
                DeliveryAddressId = request.DeliveryAddressId,
                TrackingNumber = request.TrackingNumber,
                Carrier = request.Carrier,
                Status = "Pending",
                EstimatedDeliveryDate = request.EstimatedDeliveryDate,
                IsGift = request.IsGift,
                RecipientName = request.RecipientName,
                RecipientPhone = request.RecipientPhone,
                GiftMessage = request.GiftMessage,
                Notes = request.Notes
            };

            await _shipmentRepository.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<ShipmentDto>.SuccessResponse(MapToDto(shipment), "Shipment created successfully", 201);
        }

        // Handle Get by Tracking
        public async Task<EndpointResponse<ShipmentDto>> Handle(GetShipmentByTrackingQuery request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetAll()
                .FirstOrDefaultAsync(s => s.TrackingNumber == request.TrackingNumber, cancellationToken);

            if (shipment == null)
                return EndpointResponse<ShipmentDto>.NotFoundResponse("Shipment not found");

            return EndpointResponse<ShipmentDto>.SuccessResponse(MapToDto(shipment));
        }

        // Handle Driver Location Update
        public async Task<EndpointResponse<bool>> Handle(UpdateDriverLocationCommand request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
                return EndpointResponse<bool>.NotFoundResponse("Shipment not found");

            if (shipment.Status != "OutForDelivery" && shipment.Status != "InTransit")
                return EndpointResponse<bool>.ErrorResponse("Location can only be updated for shipments in transit or out for delivery");

            shipment.DriverLatitude = request.Latitude;
            shipment.DriverLongitude = request.Longitude;
            shipment.LastLocationUpdate = DateTime.UtcNow;

            if (!string.IsNullOrEmpty(request.DriverName)) shipment.DriverName = request.DriverName;
            if (!string.IsNullOrEmpty(request.DriverPhone)) shipment.DriverPhone = request.DriverPhone;

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, "Driver location updated successfully");
        }

        // Handle Status Update
        public async Task<EndpointResponse<bool>> Handle(UpdateShipmentStatusCommand request, CancellationToken cancellationToken)
        {
            var shipment = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (shipment == null)
                return EndpointResponse<bool>.NotFoundResponse("Shipment not found");

            var validStatuses = new[] { "Pending", "InTransit", "OutForDelivery", "Delivered", "Failed" };
            if (!validStatuses.Contains(request.NewStatus))
                return EndpointResponse<bool>.ErrorResponse($"Invalid status. Valid: {string.Join(", ", validStatuses)}");

            shipment.Status = request.NewStatus;
            if (!string.IsNullOrEmpty(request.CurrentLocation)) shipment.CurrentLocation = request.CurrentLocation;
            if (!string.IsNullOrEmpty(request.Notes)) shipment.Notes = request.Notes;
            if (request.NewStatus == "Delivered") shipment.ActualDeliveryDate = DateTime.UtcNow;

            _shipmentRepository.Update(shipment);
            await _unitOfWork.SaveChangesAsync(cancellationToken);

            return EndpointResponse<bool>.SuccessResponse(true, $"Status updated to {request.NewStatus}");
        }

        // Handle Get Delivery Tracking Map Display
        public async Task<EndpointResponse<DeliveryTrackingDto>> Handle(GetDeliveryTrackingQuery request, CancellationToken cancellationToken)
        {
            var s = await _shipmentRepository.GetByIdAsync(request.ShipmentId);
            if (s == null)
                return EndpointResponse<DeliveryTrackingDto>.NotFoundResponse("Shipment not found");

            var dto = new DeliveryTrackingDto(
                s.Id,
                s.TrackingNumber,
                s.Carrier,
                s.Status,
                s.DriverLatitude,
                s.DriverLongitude,
                s.LastLocationUpdate,
                s.DriverName,
                s.DriverPhone,
                s.DriverPhotoUrl
            );

            return EndpointResponse<DeliveryTrackingDto>.SuccessResponse(dto);
        }

        private static ShipmentDto MapToDto(Shipment s)
        {
            return new ShipmentDto(
                s.Id, s.OrderId, s.DeliveryAddressId, s.TrackingNumber, s.Carrier, s.Status,
                s.EstimatedDeliveryDate, s.ActualDeliveryDate, s.CurrentLocation, s.IsGift,
                s.RecipientName, s.GiftMessage, s.CreatedAt
            );
        }
    }

    // --- MassTransit Consumers ---

    public class PaymentSucceededConsumer : IConsumer<PaymentSucceededEvent>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PaymentSucceededConsumer> _logger;

        public PaymentSucceededConsumer(
            IBaseRepository<Shipment> shipmentRepository,
            IUnitOfWork unitOfWork,
            ILogger<PaymentSucceededConsumer> logger)
        {
            _shipmentRepository = shipmentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<PaymentSucceededEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Creating shipment for Paid order: {OrderId}", msg.OrderId);

            var shipment = new Shipment
            {
                OrderId = msg.OrderId,
                DeliveryAddressId = msg.DeliveryAddressId,
                TrackingNumber = $"TRK-ONLINE-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Carrier = "Flower Delivery Express",
                Status = "Pending",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
                IsGift = msg.IsGift,
                RecipientName = msg.RecipientName,
                RecipientPhone = msg.RecipientPhone,
                GiftMessage = msg.GiftMessage,
                Notes = "Deliver after payment verification completed"
            };

            await _shipmentRepository.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Shipment created successfully for Order: {OrderId} with Tracking: {Tracking}", msg.OrderId, shipment.TrackingNumber);
        }
    }

    public class CodOrderApprovedConsumer : IConsumer<CodOrderApprovedEvent>
    {
        private readonly IBaseRepository<Shipment> _shipmentRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CodOrderApprovedConsumer> _logger;

        public CodOrderApprovedConsumer(
            IBaseRepository<Shipment> shipmentRepository,
            IUnitOfWork unitOfWork,
            ILogger<CodOrderApprovedConsumer> logger)
        {
            _shipmentRepository = shipmentRepository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task Consume(ConsumeContext<CodOrderApprovedEvent> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Creating shipment for COD order: {OrderId}", msg.OrderId);

            var shipment = new Shipment
            {
                OrderId = msg.OrderId,
                DeliveryAddressId = msg.DeliveryAddressId,
                TrackingNumber = $"TRK-COD-{Guid.NewGuid().ToString()[..8].ToUpper()}",
                Carrier = "Flower Delivery Express (COD)",
                Status = "Pending",
                EstimatedDeliveryDate = DateTime.UtcNow.AddDays(3),
                IsGift = msg.IsGift,
                RecipientName = msg.RecipientName,
                RecipientPhone = msg.RecipientPhone,
                GiftMessage = msg.GiftMessage,
                Notes = "Collect Cash On Delivery"
            };

            await _shipmentRepository.AddAsync(shipment);
            await _unitOfWork.SaveChangesAsync(context.CancellationToken);
            _logger.LogInformation("Shipment created successfully for COD Order: {OrderId} with Tracking: {Tracking}", msg.OrderId, shipment.TrackingNumber);
        }
    }
}
