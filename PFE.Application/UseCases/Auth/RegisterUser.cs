using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using PFE.Domain.Entities;

public class RegisterUser(
    IUserRepository userRepository,
    IPasswordHasher passwordHasher)
{
    public async Task<User> Execute(RegisterDto request)
    {
        // Validate department/role through UserRepository
        if (!await userRepository.DepartmentExistsAsync(request.DepartmentId))
            throw new Exception("Invalid Department");

        if (!await userRepository.RoleExistsAsync(request.RoleId))
            throw new Exception("Invalid Role");

        var user = new User
        {
            Email = request.Email,
            PasswordHash = passwordHasher.Hash(request.Password),
            Name = request.Name,
            DepartmentId = request.DepartmentId,
            RoleId = request.RoleId
        };

        return await userRepository.CreateAsync(user);
    }
}