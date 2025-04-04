namespace PFE.Application.DTOs;
public record RegisterDto(string Email, string Password, string Name);
public record LoginDto(string Email, string Password);

public record ForgotPasswordDto(string Email);
public record AuthResultDto(int UserId, string Email, string Name);