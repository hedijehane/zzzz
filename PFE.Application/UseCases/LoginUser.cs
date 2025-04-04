using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using PFE.Domain.Entities;

namespace PFE.Application.UseCases.Auth;
public class LoginUser(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
{
    public async Task<User> Execute(LoginDto request)
    {
        var user = await userRepository.GetByEmailAsync(request.Email)
            ?? throw new Exception("User not found");

        if (!passwordHasher.Verify(user.PasswordHash, request.Password))
            throw new Exception("Invalid password");

        return user;
    }
}