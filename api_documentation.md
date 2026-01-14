| Service & Feature | HTTP Method | Full Gateway URL | Endpoint Description | Required Body/Params |
|---|---|---|---|---|
| Auth Service | POST | http://72.61.102.216:8888/api/auth/change-password | Endpoint for /api/auth/change-password |  |
| Auth Service | POST | http://72.61.102.216:8888/api/auth/forget-password | Endpoint for /api/auth/forget-password | [FromBody] SendOtpCommand command |
| Auth Service | POST | http://72.61.102.216:8888/api/auth/login | Endpoint for /api/auth/login | [FromBody] LoginCommand command |
| Auth Service | POST | http://72.61.102.216:8888/api/auth/logout | Endpoint for /api/auth/logout |  |
| Auth Service | POST | http://72.61.102.216:8888/api/auth/register | Endpoint for /api/auth/register | [FromBody] RegisterDto dto |
| Auth Service | POST | http://72.61.102.216:8888/api/auth/reset-password | Endpoint for /api/auth/reset-password | [FromBody] ResetPasswordCommand command |
| Auth Service | PUT | http://72.61.102.216:8888/api/auth/update-profile | Endpoint for /api/auth/update-profile |  |
| Auth Service | GET | http://72.61.102.216:8888/api/auth/user-info | Endpoint for /api/auth/user-info |  |
| Auth Service | POST | http://72.61.102.216:8888/api/auth/verify-otp | Endpoint for /api/auth/verify-otp | [FromBody] VerifyOtpCommand command |
| Catalog (General) | PUT | http://72.61.102.216:8888/api/adoccasions/{id:int} | Endpoint for /api/adoccasions/{id:int} | int id, ActivateDeactivateOcassionDto dto |
| Catalog (General) | GET | http://72.61.102.216:8888/api/catalog/occasions | Endpoint for /api/catalog/occasions |  |
| Catalog (General) | GET | http://72.61.102.216:8888/api/categories/active | Endpoint for /api/categories/active |  |
| Catalog (General) | PUT | http://72.61.102.216:8888/api/occasions/{id:int} | Endpoint for /api/occasions/{id:int} | int id, UpdateOccasionDto dto |
| Catalog (General) | GET | http://72.61.102.216:8888/api/occasions/{occasionId:int}/products | Product operation | int occasionId |
| Catalog (General) | GET | http://72.61.102.216:8888/api/v1/categories/{categoryId}/products | Product operation | int categoryId |
| Catalog (General) | POST | http://72.61.102.216:8888/api/v1/occasions | Endpoint for /api/v1/occasions | CreateOccasionDto dto |
| Catalog (General) | POST | http://72.61.102.216:8888/api/v1/products/{productId}/reviews | Product operation | int productId, AddReviewDto dto |
| Catalog (General) | DELETE | http://72.61.102.216:8888/occasions/{id:int} | Endpoint for /occasions/{id:int} | int id, , CancellationToken ct |
| Catalog (Products) | POST | http://72.61.102.216:8888/api/categories | Endpoint for /api/categories | CreateCategoryCommand command |
| Catalog (Products) | POST | http://72.61.102.216:8888/api/products | Product operation | [FromBody] CreateProductCommand command |
| Catalog (Products) | GET | http://72.61.102.216:8888/api/products/{id:int} | Product operation | int id |
| Catalog (Products) | GET | http://72.61.102.216:8888/api/products/best-sellers | Product operation | int? count |
| Catalog (Products) | GET | http://72.61.102.216:8888/api/v1/categories | Endpoint for /api/v1/categories |  |
| Catalog (Products) | PUT | http://72.61.102.216:8888/api/v1/categories/{id} | Endpoint for /api/v1/categories/{id} | int id, UpdateCategoryDto dto |
| Catalog (Products) | POST | http://72.61.102.216:8888/api/v1/products/exists | Product operation | ProductExistsDto dto |
| Catalog (Products) | POST | http://72.61.102.216:8888/api/v1/products/search | Product operation | DTOs dto |
| Catalog (Products) | DELETE | http://72.61.102.216:8888/categories/{id:int} | Endpoint for /categories/{id:int} | int id, , CancellationToken ct |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/banners | Endpoint for /api/banners |  |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/banners | Endpoint for /api/banners | [FromBody] CreateBannerCommand command |
| Catalog (Promotions) | PUT | http://72.61.102.216:8888/api/banners/{id} | Endpoint for /api/banners/{id} | int id, [FromBody] UpdateBannerRequest request |
| Catalog (Promotions) | DELETE | http://72.61.102.216:8888/api/banners/{id} | Endpoint for /api/banners/{id} | int id |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/banners/active | Endpoint for /api/banners/active | BannerPosition? position |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/coupons | Endpoint for /api/coupons |  |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/coupons | Endpoint for /api/coupons | [FromBody] CreateCouponCommand command |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/coupons/apply | Endpoint for /api/coupons/apply | [FromBody] ApplyCouponRequest request |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/coupons/history | Endpoint for /api/coupons/history |  |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/coupons/validate | Endpoint for /api/coupons/validate | [FromBody] ValidateCouponRequest request |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/loyalty/balance | Endpoint for /api/loyalty/balance |  |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/loyalty/redeem | Endpoint for /api/loyalty/redeem | [FromBody] RedeemPointsRequest request |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/loyalty/tiers | Endpoint for /api/loyalty/tiers |  |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/loyalty/transactions | Endpoint for /api/loyalty/transactions |  |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/offers | Endpoint for /api/offers | string? status, string? sortBy |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/offers | Endpoint for /api/offers | [FromBody] CreateOfferCommand command |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/offers/{id} | Endpoint for /api/offers/{id} | int id |
| Catalog (Promotions) | DELETE | http://72.61.102.216:8888/api/offers/{id} | Endpoint for /api/offers/{id} | int id |
| Catalog (Promotions) | PUT | http://72.61.102.216:8888/api/offers/{id} | Endpoint for /api/offers/{id} | int id, [FromBody] UpdateOfferRequest request |
| Catalog (Promotions) | GET | http://72.61.102.216:8888/api/offers/active | Endpoint for /api/offers/active | int? productId, int? categoryId, int? occasionId |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/registration-codes | Endpoint for /api/registration-codes | [FromBody] CreateRegistrationCodeRequest request |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/registration-codes/apply | Endpoint for /api/registration-codes/apply | [FromBody] ApplyRegistrationCodeRequest request |
| Catalog (Promotions) | POST | http://72.61.102.216:8888/api/registration-codes/validate | Endpoint for /api/registration-codes/validate | [FromBody] ValidateRegistrationCodeRequest request |
| Notification Service | POST | http://72.61.102.216:8888/api/notifications/{id:int}/read | Endpoint for /api/notifications/{id:int}/read | int id, string userId |
| Notification Service | GET | http://72.61.102.216:8888/api/notifications/{userId} | Endpoint for /api/notifications/{userId} | string userId, string? type, bool? isRead, int pageNumber, int pageSize |
| Notification Service | POST | http://72.61.102.216:8888/api/notifications/{userId}/read-all | Endpoint for /api/notifications/{userId}/read-all | string userId |
| Ordering (Cart) | POST | http://72.61.102.216:8888/api/cart/checkout | Process Checkout | [FromBody] CheckoutRequest request |
| Ordering (Cart) | POST | http://72.61.102.216:8888/api/cart/items | Cart operation | [FromBody] AddToCartRequest request |
| Ordering (Cart) | DELETE | http://72.61.102.216:8888/api/cart/items/{productId} | Cart operation | int productId |
| Ordering (Cart) | GET | http://72.61.102.216:8888/api/v1/cart | Cart operation | HttpContext httpContext |
| Ordering (Cart) | PUT | http://72.61.102.216:8888/cart/items/{cartId:int} | Cart operation | int cartId, UpdateCartItemRequest request, , CancellationToken ct |
| Ordering (Delivery) | POST | http://72.61.102.216:8888/api/addresses | Endpoint for /api/addresses: Add item | [FromBody] CreateAddressCommand command |
| Ordering (Delivery) | DELETE | http://72.61.102.216:8888/api/addresses/{addressId} | Endpoint for /api/addresses/{addressId}: Add item | int addressId, string userId |
| Ordering (Delivery) | PUT | http://72.61.102.216:8888/api/addresses/{addressId} | Endpoint for /api/addresses/{addressId}: Add item | int addressId, [FromBody] UpdateAddressRequest request |
| Ordering (Delivery) | PUT | http://72.61.102.216:8888/api/addresses/{addressId}/default | Endpoint for /api/addresses/{addressId}/default: Add item | int addressId, string userId |
| Ordering (Delivery) | GET | http://72.61.102.216:8888/api/addresses/user/{userId} | Endpoint for /api/addresses/user/{userId}: Add item | string userId |
| Ordering (Delivery) | GET | http://72.61.102.216:8888/api/orders/{orderId:int}/tracking | Endpoint for /api/orders/{orderId:int}/tracking | int orderId, , BuildingBlocks.Interfaces.IBaseRepository<Entities.Shipment> shipmentRepo |
| Ordering (Delivery) | POST | http://72.61.102.216:8888/api/shipments | Endpoint for /api/shipments | [FromBody] CreateShipmentCommand command |
| Ordering (Delivery) | POST | http://72.61.102.216:8888/api/shipments/{id:int}/location | Endpoint for /api/shipments/{id:int}/location | int id, UpdateDriverLocationRequest request |
| Ordering (Delivery) | GET | http://72.61.102.216:8888/api/shipments/{id:int}/tracking | Endpoint for /api/shipments/{id:int}/tracking | int id |
| Ordering (Delivery) | GET | http://72.61.102.216:8888/api/shipments/{shipmentId} | Endpoint for /api/shipments/{shipmentId} | int shipmentId |
| Ordering (Delivery) | PUT | http://72.61.102.216:8888/api/shipments/{shipmentId}/status | Endpoint for /api/shipments/{shipmentId}/status | int shipmentId, [FromBody] UpdateStatusRequest request |
| Ordering (Delivery) | GET | http://72.61.102.216:8888/api/shipments/order/{orderId} | Endpoint for /api/shipments/order/{orderId} | int orderId |
| Ordering (Delivery) | GET | http://72.61.102.216:8888/api/shipments/tracking/{trackingNumber} | Endpoint for /api/shipments/tracking/{trackingNumber} | string trackingNumber |
| Ordering (Orders) | POST | http://72.61.102.216:8888/api/orders | Endpoint for /api/orders | [FromBody] CreateOrderCommand command |
| Ordering (Orders) | GET | http://72.61.102.216:8888/api/orders/{orderId} | Endpoint for /api/orders/{orderId} |  |
| Ordering (Orders) | GET | http://72.61.102.216:8888/api/orders/{orderId}/reorder-preview | Endpoint for /api/orders/{orderId}/reorder-preview | int orderId |
| Ordering (Orders) | PUT | http://72.61.102.216:8888/api/orders/{orderId}/status | Endpoint for /api/orders/{orderId}/status | int orderId, [FromBody] UpdateOrderStatusRequest request |
| Ordering (Orders) | POST | http://72.61.102.216:8888/api/orders/reorder | Endpoint for /api/orders/reorder | [FromBody] ReOrderRequest request |
| Ordering (Orders) | GET | http://72.61.102.216:8888/api/orders/user/{userId} | Endpoint for /api/orders/user/{userId} | string userId, [FromQuery] int page, [FromQuery] int pageSize, [FromQuery] string? status |
| Ordering (Orders) | POST | http://72.61.102.216:8888/api/v1/orders/{orderId}/confirm | Endpoint for /api/v1/orders/{orderId}/confirm | int orderId |
| Ordering (Orders) | GET | http://72.61.102.216:8888/api/v1/orders/my | Endpoint for /api/v1/orders/my | string? status |
| Ordering (Orders) | GET | http://72.61.102.216:8888/orders/{orderId}/status | Endpoint for /orders/{orderId}/status | int orderId, , int page = 1, int pageSize = 10, string? status = null |
| Payment Service | POST | http://72.61.102.216:8888/api/payments/create-intent | Endpoint for /api/payments/create-intent | CreatePaymentIntentRequest request |
| Payment Service | GET | http://72.61.102.216:8888/api/payments/order/{orderId:int} | Endpoint for /api/payments/order/{orderId:int} | int orderId |
| Payment Service | POST | http://72.61.102.216:8888/api/payments/process | Endpoint for /api/payments/process | ProcessPaymentRequest request |
