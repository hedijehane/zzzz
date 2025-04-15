using PFE.Application.Interfaces;
using PFE.Application.DTOs;
using System;
using System.Threading.Tasks;

namespace PFE.Application.UseCases.Auth
{
    public class ForgotPassword
    {
        private readonly IUserRepository _userRepository;
        private readonly IEmailSender _emailSender;

        public ForgotPassword(IUserRepository userRepository, IEmailSender emailSender)
        {
            _userRepository = userRepository;
            _emailSender = emailSender;
        }

        public async Task Execute(ForgotPasswordDto dto)
        {
            // Retrieve the user by email
            var user = await _userRepository.GetByEmailAsync(dto.Email);
            if (user == null) return; // Silent fail if user does not exist

            // Generate a new reset token
            var token = Guid.NewGuid().ToString();

            // Store the reset token and expiration date in the repository
            await _userRepository.StoreResetTokenAsync(user.Email, token);

            // Generate the password reset link, including the token and email
            var resetLink = $"https://localhost:7143/Auth/ResetPassword?token={token}&email={dto.Email}";

            // Send the password reset email with the reset link
            await _emailSender.SendEmailAsync(
                dto.Email,
                "Password Reset",
                $"Click <a href='{resetLink}'>here</a> to reset your password");

            // You might want to log the token generation or handle failure cases
        }
    }
}
