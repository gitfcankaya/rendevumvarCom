# Authentication System Documentation

## Overview

The RendevumVar authentication system uses JWT (JSON Web Tokens) with refresh token rotation for secure user authentication and authorization.

## Features

✅ User Registration with role-based signup  
✅ User Login with password verification  
✅ JWT Access Tokens (15-minute expiry)  
✅ Refresh Tokens (7-day expiry)  
✅ Token refresh rotation for security  
✅ Token revocation (logout)  
✅ Password hashing with BCrypt  
✅ Role-based authorization  
✅ Swagger UI integration for testing  

## Supported Roles

- **Customer**: End users who book appointments
- **Staff**: Salon employees who manage their schedules
- **BusinessOwner**: Salon owners who manage their business
- **Admin**: System administrators

## API Endpoints

### Register
```http
POST /api/auth/register
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!",
  "confirmPassword": "SecurePass123!",
  "firstName": "John",
  "lastName": "Doe",
  "phone": "+90555123456",
  "role": "Customer"
}
```

**Response:**
```json
{
  "accessToken": "eyJhbGc...",
  "refreshToken": "base64string...",
  "expiresAt": "2024-01-01T12:15:00Z",
  "user": {
    "id": "guid",
    "email": "user@example.com",
    "firstName": "John",
    "lastName": "Doe",
    "phone": "+90555123456",
    "role": "Customer",
    "profilePictureUrl": null,
    "emailConfirmed": false,
    "isActive": true
  }
}
```

### Login
```http
POST /api/auth/login
Content-Type: application/json

{
  "email": "user@example.com",
  "password": "SecurePass123!"
}
```

### Refresh Token
```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

### Logout
```http
POST /api/auth/logout
Authorization: Bearer your-access-token
Content-Type: application/json

{
  "refreshToken": "your-refresh-token"
}
```

### Forgot Password
```http
POST /api/auth/forgot-password
Content-Type: application/json

{
  "email": "user@example.com"
}
```

### Reset Password
```http
POST /api/auth/reset-password
Content-Type: application/json

{
  "email": "user@example.com",
  "token": "reset-token",
  "newPassword": "NewSecurePass123!",
  "confirmPassword": "NewSecurePass123!"
}
```

## Using Authentication in Controllers

To protect an endpoint, use the `[Authorize]` attribute:

```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize] // Requires authentication
public class MyController : ControllerBase
{
    [HttpGet]
    public IActionResult GetProtectedData()
    {
        // Only authenticated users can access this
        return Ok("Protected data");
    }
}
```

### Role-Based Authorization

```csharp
[HttpGet("admin-only")]
[Authorize(Roles = "Admin")]
public IActionResult AdminOnly()
{
    return Ok("Admin data");
}

[HttpGet("business")]
[Authorize(Roles = "BusinessOwner,Admin")]
public IActionResult BusinessData()
{
    return Ok("Business data");
}
```

## Testing with Swagger

1. Start the API: `dotnet run --project src/RendevumVar.API`
2. Open Swagger UI: `https://localhost:7000/swagger`
3. Register a new user via `/api/auth/register`
4. Copy the `accessToken` from the response
5. Click "Authorize" button in Swagger UI
6. Enter: `Bearer your-access-token`
7. Click "Authorize"
8. Now you can test protected endpoints!

## Security Features

- **Password Hashing**: BCrypt with salt rounds
- **Token Expiry**: Short-lived access tokens (15 min)
- **Refresh Token Rotation**: Old tokens are revoked when refreshed
- **Role-Based Access**: Claims-based authorization
- **HTTPS**: All endpoints use HTTPS in production
- **CORS**: Configured for frontend origins only

## Database Schema

### Users Table
- Id (Guid, Primary Key)
- TenantId (Guid, Foreign Key)
- Email (Unique)
- PasswordHash
- FirstName, LastName
- Phone
- Role (Enum)
- EmailConfirmed, PhoneConfirmed
- IsActive
- ProfilePictureUrl
- CreatedAt, UpdatedAt, IsDeleted

### RefreshTokens Table
- Id (Guid, Primary Key)
- UserId (Guid, Foreign Key)
- Token
- ExpiresAt
- RevokedAt
- IsRevoked
- CreatedAt, UpdatedAt, IsDeleted

## Configuration

In `appsettings.json`:

```json
{
  "JwtSettings": {
    "SecretKey": "your-secret-key-at-least-32-characters",
    "Issuer": "RendevumVar",
    "Audience": "RendevumVarUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  }
}
```

⚠️ **Important**: Change the `SecretKey` in production!

## Next Steps

- [ ] Implement email confirmation
- [ ] Complete forgot/reset password with email tokens
- [ ] Add rate limiting to prevent brute force
- [ ] Implement tenant resolution middleware
- [ ] Add 2FA (Two-Factor Authentication)
- [ ] Add OAuth providers (Google, Facebook)
