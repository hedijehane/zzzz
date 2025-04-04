// Intranet.Application/Commands/Auth/Handlers/LoginCommandHandler.cs
using Intranet.Application.Interfaces;
using Intranet.Domain.Entities;
using MediatR;

namespace Intranet.Application.Commands.Auth.Handlers;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResult>
{
    private readonly IUserRepository _userRepository;

    public LoginCommandHandler(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<AuthResult> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        // 1. Input validation
        if (string.IsNullOrWhiteSpace(request.Email))
            return AuthResult.Failure("Email is required");
            
        if (string.IsNullOrWhiteSpace(request.Password))
            return AuthResult.Failure("Password is required");

        // 2. Find user by email
        var user = await _userRepository.GetUserByEmailAsync(request.Email.Trim());
        if (user == null)
            return AuthResult.Failure("Invalid credentials");

        // 3. Verify password (simplified - use proper hashing in production)
        if (!PasswordService.VerifyPassword(request.Password, user.PasswordHash))
            return AuthResult.Failure("Invalid credentials");

        // 4. Return successful result
        return AuthResult.Success(user);
    }
}