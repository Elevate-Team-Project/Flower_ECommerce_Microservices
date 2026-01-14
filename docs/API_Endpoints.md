# Flower E-Commerce Microservices - API Endpoints

**Base URL:** `http://72.61.102.216:8888`

---

## 🔐 Auth Service (Gateway: `/api/auth`)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| POST | `http://72.61.102.216:8888/api/auth/api/Auth/register` | Register a new user | JSON body: `RegisterDto` (email, password, etc.) |
| POST | `http://72.61.102.216:8888/api/auth/api/Auth/login` | Authenticate user and get JWT | JSON body: `LoginCommand` (email, password) |
| GET | `http://72.61.102.216:8888/api/auth/api/Auth/user-info` | Get current authenticated user info | Bearer Token required |
| POST | `http://72.61.102.216:8888/api/auth/api/Auth/change-password` | Change user password | Bearer Token + JSON body: `ChangePasswordCommand` |
| PUT | `http://72.61.102.216:8888/api/auth/api/Auth/update-profile` | Update user profile | Bearer Token + Form data: `UpdateUserProfileRequest` |
| POST | `http://72.61.102.216:8888/api/auth/api/Auth/forget-password` | Request password reset OTP | JSON body: `SendOtpCommand` (email) |
| POST | `http://72.61.102.216:8888/api/auth/api/Auth/verify-otp` | Verify OTP code | JSON body: `VerifyOtpCommand` (email, otp) |
| POST | `http://72.61.102.216:8888/api/auth/api/Auth/reset-password` | Reset password with OTP | JSON body: `ResetPasswordCommand` (email, otp, newPassword) |
| POST | `http://72.61.102.216:8888/api/auth/api/Auth/logout` | Logout user | Bearer Token required |

---

## 📦 Catalog Service (Gateway: `/api/catalog`)

### Categories

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `http://72.61.102.216:8888/api/catalog/api/v1/categories` | Get all categories | None |
| POST | `http://72.61.102.216:8888/api/catalog/api/categories` | Create a new category | JSON body: category data |
| PUT | `http://72.61.102.216:8888/api/catalog/api/v1/categories/{id}` | Update a category | Path: `id` (int) + JSON body |
| DELETE | `http://72.61.102.216:8888/api/catalog/categories/{id}` | Delete a category | Path: `id` (int) |
| GET | `http://72.61.102.216:8888/api/catalog/api/categories/active` | Get active categories | None |
| PATCH | `http://72.61.102.216:8888/api/catalog/api/v1/categories/{id}/activate` | Activate a category | Path: `id` (int) |
| PATCH | `http://72.61.102.216:8888/api/catalog/api/v1/categories/{id}/deactivate` | Deactivate a category | Path: `id` (int) |

### Products

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `http://72.61.102.216:8888/api/catalog/api/products/{id}` | Get product details by ID | Path: `id` (int) |
| GET | `http://72.61.102.216:8888/api/catalog/api/products/best-sellers` | Get best selling products | None |
| POST | `http://72.61.102.216:8888/api/catalog/api/v1/products/search` | Search products | JSON body: search criteria |
| POST | `http://72.61.102.216:8888/api/catalog/api/v1/products/exists` | Check if products exist | JSON body: product IDs |

### Occasions

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `http://72.61.102.216:8888/api/catalog/api/catalog/occasions` | Get all occasions | None |
| POST | `http://72.61.102.216:8888/api/catalog/api/v1/occasions` | Create a new occasion | JSON body: occasion data |
| PUT | `http://72.61.102.216:8888/api/catalog/api/occasions/{id}` | Update an occasion | Path: `id` (int) + JSON body |
| DELETE | `http://72.61.102.216:8888/api/catalog/occasions/{id}` | Delete an occasion | Path: `id` (int) |
| PUT | `http://72.61.102.216:8888/api/catalog/api/adoccasions/{id}` | Activate/Deactivate occasion | Path: `id` (int) + JSON body |

---

## 🛒 Cart Service (Gateway: `/api/cart`)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `/api/cart/health` | Health check endpoint | None |
| GET | `/api/v1/cart` | View shopping cart (US-D02) | Bearer Token |
| POST | `/api/cart/items` | Add product to cart (US-D01) | JSON: `{ productId, quantity }` |
| PUT | `/api/v1/cart/items/{productId}` | Update item quantity (US-D03) | JSON: `{ quantity }` |
| DELETE | `/api/cart/items/{productId}` | Remove item from cart (US-D04) | Bearer Token |
| POST | `/api/cart/checkout` | Checkout and place order (US-D07) | JSON: `CheckoutRequest` |

### Checkout Request Body
```json
{
  "deliveryAddressId": 5,
  "shippingAddress": "...",  // Manual address (optional)
  "paymentMethod": "CashOnDelivery",  // or "CreditCard"
  "couponCode": "SAVE10",
  "notes": "Please deliver quickly",
  "isGift": true,
  "recipientName": "John",
  "recipientPhone": "+201234567890",
  "giftMessage": "Happy Birthday!"
}
```

---

## 📋 Ordering Service (Gateway: `/api/orders`)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| POST | `http://72.61.102.216:8888/api/orders/api/orders` | Create a new order | JSON body: order data |
| GET | `http://72.61.102.216:8888/api/orders/api/orders/{orderId}` | Get order details | Path: `orderId` (int) |
| GET | `http://72.61.102.216:8888/api/orders/api/orders/user/{userId}` | Get orders by user ID | Path: `userId` (string) |
| PUT | `http://72.61.102.216:8888/api/orders/api/orders/{orderId}/status` | Update order status | Path: `orderId` (int) + JSON body |
| GET | `http://72.61.102.216:8888/api/orders/api/v1/orders/my` | Get current user's orders | Query: `status` (optional: active/completed/cancelled) |
| POST | `http://72.61.102.216:8888/api/orders/api/orders/reorder` | Reorder a delivered order | JSON body: `ReOrderRequest` |
| GET | `http://72.61.102.216:8888/api/orders/api/orders/{orderId}/reorder-preview` | Preview order for reorder | Path: `orderId` (int) |

### ReOrder Request Body

```json
{
  "originalOrderId": 123,
  "items": [
    { "productId": 1, "quantity": 2 },
    { "productId": 2, "quantity": 0 }  // Set to 0 to remove item
  ],
  "deliveryAddressId": 5,              // Optional: use saved address
  "shippingAddress": "...",            // Optional: manual address
  "notes": "Please deliver quickly"    // Optional
}
```

---


## 🚚 Delivery Service (Gateway: `/api/delivery`)

### Addresses

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| POST | `http://72.61.102.216:8888/api/delivery/api/addresses` | Create a new address | JSON body: `CreateAddressCommand` |
| GET | `http://72.61.102.216:8888/api/delivery/api/addresses/user/{userId}` | Get user's addresses | Path: `userId` (string) |
| PUT | `http://72.61.102.216:8888/api/delivery/api/addresses/{addressId}` | Update an address | Path: `addressId` (int) + JSON body |
| DELETE | `http://72.61.102.216:8888/api/delivery/api/addresses/{addressId}` | Delete an address | Path: `addressId` (int) + Query: `userId` |
| PUT | `http://72.61.102.216:8888/api/delivery/api/addresses/{addressId}/default` | Set default address | Path: `addressId` (int) + Query: `userId` |

### Shipments

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| POST | `http://72.61.102.216:8888/api/delivery/api/shipments` | Create a new shipment | JSON body: `CreateShipmentCommand` |
| GET | `http://72.61.102.216:8888/api/delivery/api/shipments/{shipmentId}` | Get shipment by ID | Path: `shipmentId` (int) |
| GET | `http://72.61.102.216:8888/api/delivery/api/shipments/tracking/{trackingNumber}` | Get shipment by tracking | Path: `trackingNumber` (string) |
| GET | `http://72.61.102.216:8888/api/delivery/api/shipments/order/{orderId}` | Get shipment by order ID | Path: `orderId` (int) |
| PUT | `http://72.61.102.216:8888/api/delivery/api/shipments/{shipmentId}/status` | Update shipment status | Path: `shipmentId` (int) + JSON body |

---

## 📊 Audit Service (Gateway: `/api/audit`)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `http://72.61.102.216:8888/api/audit/` | Health check | None |

> ⚠️ **Note:** Audit Service only has a health check endpoint. Business endpoints not yet exposed.

---

## 🔔 Notification Service (Gateway: `/api/notifications`)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `http://72.61.102.216:8888/api/notifications/` | Health check | None |

> ⚠️ **Note:** Notification Service only has a health check endpoint. Business endpoints not yet exposed.

---

## 💳 Payment Service (Gateway: `/api/payment`)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `http://72.61.102.216:8888/api/payment/` | Health check | None |

> ⚠️ **Note:** Payment Service only has a health check endpoint. Business endpoints not yet exposed.

---

## 🎁 Promotion Service (Gateway: `/api/promotion`)

### Offers (Admin)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| POST | `/api/promotion/api/offers` | Create an offer (US-G01) | JSON body: `CreateOfferCommand` |
| GET | `/api/promotion/api/offers` | List all offers (US-G02) | Query: `status`, `sortBy` |
| GET | `/api/promotion/api/offers/{id}` | Get offer by ID | Path: `id` (int) |
| PUT | `/api/promotion/api/offers/{id}` | Update offer (US-G03) | Path: `id` (int) + JSON body |
| DELETE | `/api/promotion/api/offers/{id}` | Delete offer (US-G04) | Path: `id` (int) |
| GET | `/api/promotion/api/offers/active` | Get active offers (US-G05) | Query: `productId`, `categoryId`, `occasionId` |

### Coupons

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| POST | `/api/promotion/api/coupons` | Create coupon (Admin) | JSON body: `CreateCouponCommand` |
| GET | `/api/promotion/api/coupons` | List all coupons (Admin) | None |
| POST | `/api/promotion/api/coupons/validate` | Validate coupon code | JSON body: `ValidateCouponRequest` |
| POST | `/api/promotion/api/coupons/apply` | Apply coupon to order | JSON body: `ApplyCouponRequest` |
| GET | `/api/promotion/api/coupons/history` | Get user's coupon history | Bearer Token required |

### Loyalty Points (US-H01 to US-H04)

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| GET | `/api/promotion/api/loyalty/balance` | Get points balance (US-H02) | Bearer Token required |
| GET | `/api/promotion/api/loyalty/transactions` | Get points history | Bearer Token required |
| POST | `/api/promotion/api/loyalty/redeem` | Redeem points (US-H03) | JSON body: `RedeemPointsRequest` |
| GET | `/api/promotion/api/loyalty/tiers` | Get tier info (US-H04) | None |

### Banners

| HTTP Method | Full URL | Description | Required Body/Params |
|-------------|----------|-------------|---------------------|
| POST | `/api/promotion/api/banners` | Create banner (Admin) | JSON body: `CreateBannerCommand` |
| GET | `/api/promotion/api/banners` | List all banners (Admin) | None |
| GET | `/api/promotion/api/banners/active` | Get active banners | Query: `position` |
| PUT | `/api/promotion/api/banners/{id}` | Update banner | Path: `id` (int) + JSON body |
| DELETE | `/api/promotion/api/banners/{id}` | Delete banner | Path: `id` (int) |

---

## 📝 Summary

| Service | Total Endpoints |
|---------|-----------------|
| Auth | 9 |
| Catalog | 16 |
| Cart | 6 |
| Ordering | 7 |
| Delivery | 10 |
| Promotion | 21 |
| Audit | 1 |
| Notification | 1 |
| Payment | 1 |
| **Total** | **68** |


