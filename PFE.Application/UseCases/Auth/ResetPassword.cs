using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using System.ComponentModel.DataAnnotations;

public class ResetPassword
{
    private readonly IUserRepository _userRepository;

    public ResetPassword(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task Execute(ResetPasswordDto dto)
    {
        // Validate DTO using Data Annotations (if model binding not used)
        Validator.ValidateObject(dto, new ValidationContext(dto), validateAllProperties: true);

        // Manually check that the passwords match
        if (dto.NewPassword != dto.ConfirmPassword)
        {
            throw new ValidationException("Passwords do not match");
        }

        // Fetch and validate user
        var user = await _userRepository.GetByEmailAsync(dto.Email)
            ?? throw new ValidationException("User not found");

        if (user.ResetToken != dto.Token)
            throw new ValidationException("Invalid reset token");

        if (user.ResetTokenExpiry < DateTime.UtcNow)
            throw new ValidationException("Reset token has expired");

        // Update password and clear reset token
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
        user.ResetToken = null;
        user.ResetTokenExpiry = null;

        await _userRepository.UpdateAsync(user);
    }
}