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

---

## Advanced Authentication Features (Phase 2)

### 8. QR Code Authentication

For salon-based invitation system, customers can scan QR codes displayed in salons to create connections.

#### QR Code Generation Flow

```csharp
public class QRAuthenticationService : IQRAuthenticationService
{
    public async Task<QRCodeAuthResult> GenerateQRCodeAsync(Guid tenantId, Guid businessUserId)
    {
        // Generate secure token
        var token = GenerateSecureToken();
        var expiresAt = DateTime.UtcNow.AddHours(24);
        
        // Create invitation record
        var invitation = new InvitationCode
        {
            TenantId = tenantId,
            Code = GenerateShortCode(),
            Token = token,
            Type = InvitationType.QR,
            CreatedByUserId = businessUserId,
            ExpiresAt = expiresAt,
            IsActive = true
        };
        
        await _context.InvitationCodes.AddAsync(invitation);
        await _context.SaveChangesAsync();
        
        // Generate QR code image
        var invitationUrl = $"https://rendevumvar.com/invite/{token}";
        var qrCodeBytes = _qrCodeGenerator.Generate(invitationUrl, 300, 300);
        
        // Upload to blob storage
        var imageUrl = await _blobStorage.UploadAsync(
            qrCodeBytes, 
            $"qr-codes/{tenantId}/{token}.png", 
            "image/png"
        );
        
        return new QRCodeAuthResult
        {
            QRCodeImageUrl = imageUrl,
            InvitationUrl = invitationUrl,
            Token = token,
            ExpiresAt = expiresAt
        };
    }
    
    private string GenerateSecureToken()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[32];
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes)
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");
    }
}
```

#### QR Code Validation API

```http
POST /api/auth/invitation/validate
Content-Type: application/json

{
  "token": "invitation-token-from-qr"
}
```

**Response:**
```json
{
  "isValid": true,
  "tenantId": "guid",
  "businessName": "Salon Name",
  "businessLogo": "https://...",
  "expiresAt": "2024-01-02T12:00:00Z",
  "requiresRegistration": true
}
```

### 9. SMS Verification

For phone-based authentication and invitation acceptance.

#### Send SMS Verification Code

```http
POST /api/auth/sms/send-code
Content-Type: application/json

{
  "phoneNumber": "+905551234567",
  "purpose": "Registration"
}
```

**Response:**
```json
{
  "success": true,
  "message": "Verification code sent",
  "expiresInSeconds": 300,
  "codeLength": 6
}
```

#### Verify SMS Code

```http
POST /api/auth/sms/verify-code
Content-Type: application/json

{
  "phoneNumber": "+905551234567",
  "code": "123456"
}
```

**SMS Verification Service Implementation:**

```csharp
public class SMSVerificationService : ISMSVerificationService
{
    private readonly ISMSProvider _smsProvider;
    private readonly IDistributedCache _cache;
    
    public async Task<SMSVerificationResult> SendVerificationCodeAsync(
        string phoneNumber, 
        VerificationPurpose purpose)
    {
        var code = GenerateNumericCode(6);
        var cacheKey = $"sms-verification:{phoneNumber}:{purpose}";
        
        await _cache.SetStringAsync(
            cacheKey, 
            JsonSerializer.Serialize(new { Code = code, AttemptCount = 0 }),
            new DistributedCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            }
        );
        
        var message = purpose switch
        {
            VerificationPurpose.Registration => $"RendevumVar doğrulama kodunuz: {code}",
            VerificationPurpose.Login => $"Giriş kodunuz: {code}",
            VerificationPurpose.PasswordReset => $"Şifre sıfırlama kodunuz: {code}",
            _ => throw new ArgumentException("Invalid purpose")
        };
        
        await _smsProvider.SendAsync(phoneNumber, message);
        return new SMSVerificationResult { Success = true };
    }
}
```

### 10. OAuth Integration (Google Calendar)

For Google Calendar synchronization, users authorize the application.

#### Initiate Google OAuth Flow

```http
GET /api/auth/oauth/google/authorize?userId={userId}
```

**Google OAuth Configuration:**
```json
{
  "GoogleOAuth": {
    "ClientId": "your-client-id.apps.googleusercontent.com",
    "ClientSecret": "your-client-secret",
    "RedirectUri": "https://rendevumvar.com/api/auth/oauth/google/callback",
    "Scopes": [
      "https://www.googleapis.com/auth/calendar",
      "https://www.googleapis.com/auth/calendar.events"
    ]
  }
}
```

### 11. Multi-Tenant Token Claims

JWT tokens include tenant information for multi-tenant isolation.

```csharp
public async Task<string> GenerateAccessTokenAsync(User user, Guid? tenantId = null)
{
    var claims = new List<Claim>
    {
        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
        new Claim(ClaimTypes.Email, user.Email),
        new Claim(ClaimTypes.Role, user.Role.ToString())
    };
    
    if (tenantId.HasValue)
    {
        claims.Add(new Claim("tenantId", tenantId.Value.ToString()));
        
        var subscription = await _context.TenantSubscriptions
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId.Value);
        
        if (subscription != null)
        {
            claims.Add(new Claim("subscriptionPlan", subscription.Plan.Name));
        }
    }
    
    var permissions = await GetUserPermissionsAsync(user.Id, tenantId);
    foreach (var permission in permissions)
    {
        claims.Add(new Claim("permission", permission));
    }
    
    // Generate JWT token...
}
```

### 12. Biometric Authentication (Mobile PWA)

Support fingerprint and Face ID for mobile devices.

#### Enable Biometric Authentication

```http
POST /api/auth/biometric/enable
Authorization: Bearer access-token
Content-Type: application/json

{
  "deviceId": "unique-device-identifier",
  "publicKey": "base64-encoded-public-key"
}
```

### 13. Session Management

Track active user sessions across devices.

**Database Schema:**
```sql
CREATE TABLE UserSessions (
    Id INT PRIMARY KEY IDENTITY(1,1),
    UserId UNIQUEIDENTIFIER NOT NULL,
    DeviceId NVARCHAR(100) NOT NULL,
    DeviceName NVARCHAR(200) NULL,
    DeviceType NVARCHAR(50) NULL,
    RefreshTokenId UNIQUEIDENTIFIER NOT NULL,
    IsActive BIT DEFAULT 1,
    LastActivityAt DATETIME2 DEFAULT GETUTCDATE(),
    CreatedAt DATETIME2 DEFAULT GETUTCDATE()
);
```

#### List Active Sessions

```http
GET /api/auth/sessions
Authorization: Bearer access-token
```

### 14. Security Enhancements

#### Rate Limiting

```csharp
[RateLimit(MaxRequests = 5, WindowSeconds = 60)]
[HttpPost("login")]
public async Task<IActionResult> Login(LoginRequest request)
{
    // Login logic...
}
```

#### Account Lockout

```csharp
public async Task<LoginResult> LoginAsync(string email, string password)
{
    var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    
    if (user?.LockoutEnd.HasValue == true && user.LockoutEnd.Value > DateTime.UtcNow)
    {
        return new LoginResult 
        { 
            Success = false, 
            Error = "Account locked due to multiple failed attempts" 
        };
    }
    
    if (!_passwordHasher.Verify(password, user.PasswordHash))
    {
        user.FailedLoginAttempts++;
        
        if (user.FailedLoginAttempts >= 5)
        {
            user.LockoutEnd = DateTime.UtcNow.AddMinutes(30);
        }
        
        await _context.SaveChangesAsync();
        return new LoginResult { Success = false };
    }
    
    user.FailedLoginAttempts = 0;
    user.LockoutEnd = null;
    await _context.SaveChangesAsync();
    
    return new LoginResult { Success = true };
}
```

---

## Security Best Practices

1. **Never store passwords in plain text** - Always use BCrypt or Argon2
2. **Use HTTPS in production** - All authentication endpoints must use TLS
3. **Implement rate limiting** - Prevent brute force attacks
4. **Enable account lockout** - Lock accounts after failed login attempts
5. **Use strong JWT secret keys** - Minimum 32 characters, random
6. **Short access token expiry** - 15 minutes maximum
7. **Rotate refresh tokens** - Invalidate old tokens on refresh
8. **Log authentication events** - Track login attempts, failures, lockouts

