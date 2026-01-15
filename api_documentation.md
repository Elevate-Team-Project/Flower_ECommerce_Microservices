# Comprehensive API Documentation List

**Base URL:** `http://72.61.102.216:8888`

## 1. Auth Service
| Service & Feature | HTTP Method | Full Gateway URL | Endpoint Description | Required Body/Params |
| :--- | :--- | :--- | :--- | :--- |
| **Auth** | POST | `/api/auth/api/Auth/register` | Register a new user | `RegisterDto` (JSON) |
| **Auth** | POST | `/api/auth/api/Auth/login` | Login user | `LoginCommand` (Email, Password) |
| **Auth** | GET | `/api/auth/api/Auth/user-info` | Get current user info | Headers: `Authorization: Bearer <token>` |
| **Auth** | POST | `/api/auth/api/Auth/change-password` | Change password | `ChangePasswordCommand`, Auth Header |
| **Auth** | PUT | `/api/auth/api/Auth/update-profile` | Update user profile | `UpdateUserProfileRequest` (Form Data), Auth Header |
| **Auth** | POST | `/api/auth/api/Auth/forget-password` | Request OTP for password reset | `SendOtpCommand` (Email) |
| **Auth** | POST | `/api/auth/api/Auth/verify-otp` | Verify OTP | `VerifyOtpCommand` (Email, OTP) |
| **Auth** | POST | `/api/auth/api/Auth/reset-password` | Reset password | `ResetPasswordCommand` (Email, NewPassword) |
| **Auth** | POST | `/api/auth/api/Auth/logout` | Logout | Auth Header |

## 2. Catalog Service
| Service & Feature | HTTP Method | Full Gateway URL | Endpoint Description | Required Body/Params |
| :--- | :--- | :--- | :--- | :--- |
| **Catalog / Products** | GET | `/api/catalog/api/products/{id}` | Get product details | Path: `id` |
| **Catalog / Products** | GET | `/api/catalog/api/products/best-sellers` | Get best sellers | None |
| **Catalog / Products** | POST | `/api/catalog/api/v1/products/search` | Search products | `SearchProductQuery` (JSON) |
| **Catalog / Products** | POST | `/api/catalog/api/v1/products/exists` | Check product existence | List of IDs |
| **Catalog / Products** | POST | `/api/catalog/api/products` | Create product | `CreateProductCommand` |
| **Catalog / Products** | PUT | `/api/catalog/api/products` | Update product | `UpdateProductCommand` |
| **Catalog / Products** | DELETE | `/api/catalog/api/products/{id}` | Delete product | Path: `id` |
| **Catalog / Inventory** | GET | `/api/catalog/api/inventory` | Get inventory summary | Query: `status` |
| **Catalog / Offers** | GET | `/api/catalog/api/offers` | Get all offers | None |
| **Catalog / Offers** | GET | `/api/catalog/api/offers/active` | Get active offers | None |
| **Catalog / Offers** | GET | `/api/catalog/api/offers/{id}` | Get offer by ID | Path: `id` |
| **Catalog / Offers** | POST | `/api/catalog/api/offers` | Create offer | `CreateOfferCommand` |
| **Catalog / Offers** | PUT | `/api/catalog/api/offers/{id}` | Update offer | Path: `id`, Body: `UpdateOfferDto` |
| **Catalog / Offers** | DELETE | `/api/catalog/api/offers/{id}` | Delete offer | Path: `id` |
| **Catalog / Coupons** | GET | `/api/catalog/api/coupons` | Get all coupons | None |
| **Catalog / Coupons** | GET | `/api/catalog/api/coupons/history` | Get coupon history | None |
| **Catalog / Coupons** | POST | `/api/catalog/api/coupons` | Create coupon | `CreateCouponCommand` |
| **Catalog / Coupons** | POST | `/api/catalog/api/coupons/validate` | Validate coupon | `ValidateCouponQuery` |
| **Catalog / Coupons** | POST | `/api/catalog/api/coupons/apply` | Apply coupon | `ApplyCouponCommand` |
| **Catalog / Loyalty** | GET | `/api/catalog/api/loyalty/balance` | Get loyalty balance | Auth Header |
| **Catalog / Loyalty** | GET | `/api/catalog/api/loyalty/tiers` | Get loyalty tiers | None |
| **Catalog / Loyalty** | GET | `/api/catalog/api/loyalty/transactions` | Get loyalty transactions | Auth Header |
| **Catalog / Loyalty** | POST | `/api/catalog/api/loyalty/redeem` | Redeem points | `RedeemPointsCommand` |
| **Catalog / Registration Codes** | POST | `/api/catalog/api/registration-codes` | Create registration code | `CreateRegistrationCodeCommand` |
| **Catalog / Registration Codes** | POST | `/api/catalog/api/registration-codes/validate` | Validate code | `ValidateRegistrationCodeQuery` |
| **Catalog / Registration Codes** | POST | `/api/catalog/api/registration-codes/apply` | Apply code | `ApplyRegistrationCodeCommand` |
| **Catalog / Banners** | GET | `/api/catalog/api/banners` | Get all banners | None |
| **Catalog / Banners** | GET | `/api/catalog/api/banners/active` | Get active banners | None |
| **Catalog / Banners** | POST | `/api/catalog/api/banners` | Create banner | `CreateBannerCommand` |
| **Catalog / Banners** | PUT | `/api/catalog/api/banners/{id}` | Update banner | Path: `id`, Body: Data |
| **Catalog / Banners** | DELETE | `/api/catalog/api/banners/{id}` | Delete banner | Path: `id` |
| **Catalog / Categories** | GET | `/api/catalog/api/v1/categories` | Get all categories | None |
| **Catalog / Categories** | GET | `/api/catalog/api/categories/active` | Get active categories | None |
| **Catalog / Categories** | POST | `/api/catalog/api/categories` | Create category | `CreateCategoryCommand` |
| **Catalog / Categories** | PUT | `/api/catalog/api/v1/categories/{id}` | Update category | Path: `id` |
| **Catalog / Categories** | DELETE | `/api/catalog/categories/{id}` | Delete category | Path: `id` (Note: No /api prefix in source?) |
| **Catalog / Categories** | GET | `/api/catalog/api/v1/categories/{categoryId}/products` | Get products by category | Path: `categoryId` |
| **Catalog / Occasions** | GET | `/api/catalog/api/v1/occasions` | Get all occasions | None |
| **Catalog / Occasions** | POST | `/api/catalog/api/v1/occasions` | Create occasion | `CreateOccasionCommand` |
| **Catalog / Occasions** | PUT | `/api/catalog/api/occasions/{id}` | Update occasion | Path: `id` |
| **Catalog / Occasions** | PUT | `/api/catalog/api/adoccasions/{id}` | Activate/Deactivate occasion | Path: `id`, Body: `ActivateDeactivateOcassionDto` |
| **Catalog / Occasions** | DELETE | `/api/catalog/occasions/{id}` | Delete occasion | Path: `id` (Note: No /api prefix in source?) |
| **Catalog / Reviews** | POST | `/api/catalog/api/v1/products/{productId}/reviews` | Add product review | Path: `productId` |

## 3. Ordering Service
| Service & Feature | HTTP Method | Full Gateway URL | Endpoint Description | Required Body/Params |
| :--- | :--- | :--- | :--- | :--- |
| **Ordering / Orders** | POST | `/api/orders/api/orders` | Create order | `CreateOrderCommand` |
| **Ordering / Orders** | GET | `/api/orders/api/orders/{orderId}` | Get order details | Path: `orderId` |
| **Ordering / Orders** | GET | `/api/orders/api/orders/user/{userId}` | Get user orders | Path: `userId` (or Auth) |
| **Ordering / Orders** | GET | `/api/orders/api/v1/orders/my` | Get my orders | Auth Header |
| **Ordering / Orders** | POST | `/api/orders/api/v1/orders/{orderId}/confirm` | Confirm order | Path: `orderId` |
| **Ordering / Orders** | PUT | `/api/orders/api/orders/{orderId}/status` | Update order status | Path: `orderId`, Body: Status |
| **Ordering / Orders** | GET | `/api/orders/orders/{orderId}/status` | Track order status | Path: `orderId` (Note: missing /api prefix in source?) |
| **Ordering / Orders** | POST | `/api/orders/api/orders/reorder` | Re-order | OrderId/Items |
| **Ordering / Orders** | GET | `/api/orders/api/orders/{orderId}/reorder-preview` | Preview re-order | Path: `orderId` |
| **Ordering / Cart** | GET | `/api/cart/api/v1/cart` | View shopping cart | Auth/Session |
| **Ordering / Cart** | POST | `/api/cart/api/cart/items` | Add item to cart | `AddToCartCommand` |
| **Ordering / Cart** | PUT | `/api/cart/cart/items/{cartId}` | Update cart item | Path: `cartId` (Note: missing /api prefix in source?) |
| **Ordering / Cart** | DELETE | `/api/cart/api/cart/items/{productId}` | Remove item from cart | Path: `productId` |
| **Ordering / Cart** | POST | `/api/cart/api/cart/checkout` | Checkout cart | `CheckoutCommand` |
| **Ordering / Delivery** | GET | `/api/delivery/api/addresses/user/{userId}` | Get user addresses | Path: `userId` |
| **Ordering / Delivery** | POST | `/api/delivery/api/addresses` | Create address | `CreateAddressCommand` |
| **Ordering / Delivery** | PUT | `/api/delivery/api/addresses/{addressId}` | Update address | Path: `addressId` |
| **Ordering / Delivery** | PUT | `/api/delivery/api/addresses/{addressId}/default` | Set default address | Path: `addressId` |
| **Ordering / Delivery** | DELETE | `/api/delivery/api/addresses/{addressId}` | Delete address | Path: `addressId` |
| **Ordering / Delivery** | POST | `/api/delivery/api/shipments` | Create shipment | `CreateShipmentCommand` |
| **Ordering / Delivery** | GET | `/api/delivery/api/shipments/{shipmentId}` | Get shipment details | Path: `shipmentId` |
| **Ordering / Delivery** | GET | `/api/delivery/api/shipments/tracking/{trackingNumber}` | Track shipment | Path: `trackingNumber` |
| **Ordering / Delivery** | GET | `/api/delivery/api/shipments/{id}/tracking` | Get delivery tracking | Path: `id` |
| **Ordering / Delivery** | PUT | `/api/delivery/api/shipments/{shipmentId}/status` | Update shipment status | Path: `shipmentId` |
| **Ordering / Delivery** | POST | `/api/delivery/api/shipments/{id}/location` | Update driver location | Path: `id` |

## 4. Payment Service
| Service & Feature | HTTP Method | Full Gateway URL | Endpoint Description | Required Body/Params |
| :--- | :--- | :--- | :--- | :--- |
| **Payment** | POST | `/api/payment/api/payments/create-intent` | Create payment intent | `CreatePaymentIntentRequest` |
| **Payment** | POST | `/api/payment/api/payments/process` | Process payment | `ProcessPaymentRequest` |
| **Payment** | GET | `/api/payment/api/payments/order/{orderId}` | Get payment status | Path: `orderId` |

## 5. Notification Service
| Service & Feature | HTTP Method | Full Gateway URL | Endpoint Description | Required Body/Params |
| :--- | :--- | :--- | :--- | :--- |
| **Notification** | GET | `/api/notifications/api/notifications/{userId}` | Get user notifications | Path: `userId` |
| **Notification** | POST | `/api/notifications/api/notifications/{id}/read` | Mark notification read | Path: `id` |
| **Notification** | POST | `/api/notifications/api/notifications/{userId}/read-all` | Mark all read | Path: `userId` |

---
**Note:** Some endpoints in the source code have inconsistent prefixes (e.g. some missing `/api`). The Full Gateway URL assumes the standard `/api` prefix presence unless verified otherwise. Where `path:/api/x` is stripped by Gateway and Service expects `path:/api/x`, the URL effectively duplicates the segment (e.g. `/api/auth/api/auth/...`).
