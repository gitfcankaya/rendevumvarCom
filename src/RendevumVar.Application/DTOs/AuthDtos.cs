namespace RendevumVar.Application.DTOs;

public record RegisterDto(
    string Email,
    string Password,
    string ConfirmPassword,
    string FirstName,
    string LastName,
    string? Phone,
    string Role // "Customer", "BusinessOwner", "Staff"
);

public record LoginDto(
    string Email,
    string Password
);

public record AuthResponseDto(
    string AccessToken,
    string RefreshToken,
    DateTime ExpiresAt,
    UserDto User
);

public record UserDto(
    Guid Id,
    string Email,
    string FirstName,
    string LastName,
    string? Phone,
    string Role,
    string? ProfilePictureUrl,
    bool EmailConfirmed,
    bool IsActive
);

public record RefreshTokenDto(
    string RefreshToken
);

public record ForgotPasswordDto(
    string Email
);

public record ResetPasswordDto(
    string Email,
    string Token,
    string NewPassword,
    string ConfirmPassword
);
