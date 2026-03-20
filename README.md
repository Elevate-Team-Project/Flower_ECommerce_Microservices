# Flower E-Commerce Microservices

A robust, scalable, and fully functional E-Commerce platform designed specifically for a flower and gift shop. This system is built using a modern **Microservices Architecture** with .NET Core, Docker, and various message brokers and databases to ensure high availability, fault tolerance, and loose coupling.

## 🏗 System Architecture

The project is structured as a collection of independent, specialized microservices that communicate with each other asynchronously via message queues or synchronously via gRPC/HTTP.

### Technology Stack
*   **Backend Framework:** .NET Core / ASP.NET Core
*   **API Gateway:** Ocelot (or similar .NET API Gateway)
*   **Database:** Microsoft SQL Server (2022)
*   **Message Broker:** RabbitMQ (for asynchronous event-driven communication)
*   **Caching:** Redis (for distributed caching and session management)
*   **Containerization:** Docker & Docker Compose
*   **Synchronous Communication:** gRPC (e.g., Ordering Service to Catalog Service)
*   **Payment Gateway:** Stripe Integration

### Microservices Overview

The platform consists of 10 primary microservices, each with its own dedicated database and responsibility:

1.  **API Gateway (Port 8888):** The single entry point for all client requests. It routes HTTP requests to the appropriate backend microservices, handles cross-cutting concerns (like authentication routing), and aggregating responses if necessary.
2.  **Auth Service (Port 8082):** Manages user identity, registration, authentication (JWT), profile updates, and password resets via OTP.
3.  **Catalog Service (Port 8084):** Handles the product catalog, including managing products, categories, occasions, inventory tracking, reviews, promotional banners, coupons, offers, and a loyalty points system.
4.  **Cart Service (Port 8086):** Manages user shopping carts, adding/removing items, and preparing the cart for checkout.
5.  **Ordering Service (Port 8088):** Manages the entire lifecycle of an order. Handles order creation, status tracking, re-ordering functionality, user addresses, and delivery shipment tracking.
6.  **Payment Service (Port 8090):** Integrates with external payment providers (e.g., Stripe) to process payments securely and manage payment intents and statuses.
7.  **Delivery Service (Port 8092):** Handles dispatching, shipment tracking, and driver location updates.
8.  **Promotion Service (Port 8094):** Handles complex promotional rules, discounts, and marketing campaigns (often overlaps or interacts closely with the Catalog service's coupon/offer systems).
9.  **Notification Service (Port 8096):** Listens to events from RabbitMQ and sends notifications (emails, SMS, or in-app) to users regarding order updates, OTPs, or promotions.
10. **Audit Service (Port 8098):** Tracks system-wide events and logs for compliance, security monitoring, and debugging.

## ✨ Key Features

*   **Comprehensive User Authentication:** Secure registration, login, profile management, and OTP-based password recovery.
*   **Rich Product Catalog:** Browse products by categories or occasions, view best sellers, and search functionality. Includes inventory management.
*   **Shopping Cart & Checkout:** Seamless cart management with session persistence and a streamlined checkout process.
*   **Order Management:** Track order statuses in real-time, view order history, and one-click re-ordering.
*   **Delivery Tracking:** Real-time shipment tracking and driver location updates.
*   **Promotions & Loyalty:** Robust coupon application, special offers, promotional banners, and a comprehensive loyalty points system (earning and redeeming points).
*   **Secure Payments:** Integration with Stripe for secure and reliable payment processing.
*   **Event-Driven Asynchronous Communication:** Services are decoupled using RabbitMQ. For example, when an order is placed, an event is published for the Payment, Inventory, and Notification services to react independently.

## 🚀 Getting Started (Local Development)

The entire infrastructure and all microservices can be spun up easily using Docker Compose.

### Prerequisites
*   [Docker Desktop](https://www.docker.com/products/docker-desktop) installed and running.
*   .NET SDK (if you plan to build or run services outside of Docker).

### Running the System

1.  Clone the repository.
2.  Navigate to the root directory containing the `docker-compose.yml` file.
3.  Run the following command to build and start all containers in detached mode:

    ```bash
    docker-compose up -d --build
    ```

4.  Docker Compose will provision the following infrastructure containers first:
    *   `flower_sql_server` (SQL Server on port 1433)
    *   `flower_rabbitmq` (RabbitMQ on ports 5672, 15672)
    *   `flower_redis` (Redis on port 6379)
5.  Once the infrastructure is healthy, the microservices will start automatically.

### Accessing the Application
*   **API Gateway:** `http://localhost:8888` (All API requests should be routed through here).
*   **RabbitMQ Management UI:** `http://localhost:15672` (Username: `guest`, Password: `guest`).

## 📖 API Documentation

A comprehensive list of API endpoints routed through the API Gateway is available in the [`api_documentation.md`](./api_documentation.md) file. This document details the HTTP methods, full gateway URLs, endpoint descriptions, and required payloads for all major services (Auth, Catalog, Ordering, Payment, Notification).

## 🛠 Database Initialization

The SQL Server container (`flower_sql_server`) starts with a predefined SA password (`MyComplexP@ssw0rd2025`). Each microservice uses Entity Framework Core to automatically apply migrations and create its respective database (e.g., `GiftShop_IdentityDB`, `GiftShop_CatalogDB`, etc.) upon startup.
