using System.ComponentModel.DataAnnotations;

namespace PFE.Application.DTOs;

// Registration
public record RegisterDto
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int DepartmentId { get; init; }
    public int RoleId { get; init; }    
}

// Login
public record LoginDto(string Email, string Password);

// Forgot Password
public record ForgotPasswordDto(
    [Required][EmailAddress] string Email
);

// Add this new DTO for password reset
public record ResetPasswordDto(
    [Required] string Email,
    [Required] string Token,
    [Required] string NewPassword,
    [Required] string ConfirmPassword
);


// Auth result
public record AuthResultDto(
    int UserId,
    string Email,
    string Name,
    DepartmentDto Department,
    RoleDto Role
);

// Supporting DTOs
public record DepartmentDto(int Id, string Name);
public record RoleDto(int Id, string Name);