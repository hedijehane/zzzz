// Intranet.Application/Commands/Auth/Handlers/RegisterCommandHandler.cs
using Intranet.Application.Features.Auth.Register;
using Intranet.Application.Interfaces;
using Intranet.Domain;
using Intranet.Domain.Entities;
using MediatR;

namespace Intranet.Application.Commands.Auth.Handlers;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;

    public RegisterCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResult> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        // 1. Input validation (same pattern as Login)
        if (string.IsNullOrWhiteSpace(request.Email))
            return AuthResult.Failure("Email is required");

        if (string.IsNullOrWhiteSpace(request.Password))
            return AuthResult.Failure("Password is required");

        // 2. Check for existing user (mirrors login's user lookup)
        var existingUser = await _userRepository.GetUserByEmailAsync(request.Email.Trim());
        if (existingUser != null)
            return AuthResult.Failure("Email already registered");

        // 3. Create user (same password handling as Login)
        var user = new User
        {
            Email = request.Email.Trim(),
            PasswordHash = PasswordService.HashPassword(request.Password)
            IsActive = true
        };

        // 4. Save user (completes the flow like Login's success path)
        await _userRepository.AddUserAsync(user);

        // 5. Return result (identical success pattern)
        return AuthResult.Success(user);
    }
}