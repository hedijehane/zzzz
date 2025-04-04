using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using PFE.Domain.Entities;

namespace PFE.Application.UseCases.Auth;

public class RegisterUser(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
{
    public async Task<User> Execute(RegisterDto request)
    {
        // Add this single validation line
        if (!request.Email.EndsWith("@astree.com.tn", StringComparison.OrdinalIgnoreCase))
            throw new Exception("Only @astree.com .tn email addresses are allowed");

        // Keep all existing code below exactly the same
        if (await userRepository.GetByEmailAsync(request.Email) is not null)
            throw new Exception("Email already exists");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Name = request.Name
        };

        return await userRepository.CreateAsync(user);
    }
}