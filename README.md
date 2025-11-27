# OrderManagement

Project for Microsoft Back-End Developer Course

## Overview

A complete Order Management System built with ASP.NET Core Web API, featuring:

- **Entity Framework Core** with SQLite database
- **ASP.NET Identity** for authentication
- **JWT Token-based authentication**
- **Role-based access control** (Admin and User roles)
- **Memory caching** for improved performance
- **RESTful API** with GET, POST, DELETE operations

## Features

### Data Models
- **Product** - Inventory management with name, description, price, stock quantity, and category
- **Order** - Customer orders with status tracking and shipping information
- **OrderItem** - Line items connecting orders to products
- **ApplicationUser** - Extended Identity user with additional profile information

### Business Logic
- Automatic stock quantity management when orders are created or deleted
- Order status tracking (Pending, Processing, Shipped, Delivered, Cancelled)
- Transaction support for order creation
- Memory caching for product and order queries

## API Endpoints

### Authentication (`/api/auth`)

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| POST | `/register` | Register a new user | Public |
| POST | `/login` | Login and get JWT token | Public |
| GET | `/users` | Get all users | Admin |
| GET | `/users/{userId}` | Get user by ID | Admin |
| POST | `/users/{userId}/roles/{role}` | Assign role to user | Admin |

### Products (`/api/products`)

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| GET | `/` | Get all products | Public |
| GET | `/{id}` | Get product by ID | Public |
| GET | `/category/{category}` | Get products by category | Public |
| POST | `/` | Create new product | Admin |
| PUT | `/{id}` | Update product | Admin |
| DELETE | `/{id}` | Delete product | Admin |
| PATCH | `/{id}/stock` | Update product stock | Admin |

### Orders (`/api/orders`)

| Method | Endpoint | Description | Access |
|--------|----------|-------------|--------|
| GET | `/` | Get all orders | Admin |
| GET | `/my-orders` | Get current user's orders | Authenticated |
| GET | `/{id}` | Get order by ID | Owner/Admin |
| POST | `/` | Create new order | Authenticated |
| PATCH | `/{id}/status` | Update order status | Admin |
| DELETE | `/{id}` | Delete order (Pending/Cancelled only) | Owner/Admin |

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQLite (included)

### Running the Application

```bash
cd src/OrderManagement
dotnet run
```

The API will be available at `http://localhost:5000` (HTTP) and `https://localhost:5001` (HTTPS).

### Configuration

#### Development
JWT key is configured in `appsettings.Development.json` for development use.

#### Production
Set the `JWT_KEY` environment variable with a secure key (minimum 32 characters):

```bash
export JWT_KEY="YourProductionSecretKeyAtLeast32Chars!"
```

### Example Usage

#### Register a User
```bash
curl -X POST http://localhost:5000/api/auth/register \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!","firstName":"John","lastName":"Doe"}'
```

#### Login
```bash
curl -X POST http://localhost:5000/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{"email":"user@example.com","password":"Password123!"}'
```

#### Get Products
```bash
curl http://localhost:5000/api/products
```

#### Create Order (with JWT token)
```bash
curl -X POST http://localhost:5000/api/orders \
  -H "Content-Type: application/json" \
  -H "Authorization: Bearer <your-jwt-token>" \
  -d '{"shippingAddress":"123 Main St","items":[{"productId":1,"quantity":2}]}'
```

## Project Structure

```
src/OrderManagement/
├── Controllers/
│   ├── AuthController.cs
│   ├── OrdersController.cs
│   └── ProductsController.cs
├── Data/
│   └── ApplicationDbContext.cs
├── DTOs/
│   ├── AuthDtos.cs
│   ├── OrderDtos.cs
│   └── ProductDtos.cs
├── Models/
│   ├── ApplicationUser.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Product.cs
├── Services/
│   ├── AuthService.cs
│   ├── IAuthService.cs
│   ├── IOrderService.cs
│   ├── IProductService.cs
│   ├── OrderService.cs
│   └── ProductService.cs
├── Program.cs
├── appsettings.json
└── appsettings.Development.json
```

## Security Features

- JWT token authentication with configurable expiration
- Password requirements (uppercase, lowercase, digit, minimum 6 characters)
- Role-based authorization (Admin, User)
- Input validation on all endpoints
- Protection against negative stock values
