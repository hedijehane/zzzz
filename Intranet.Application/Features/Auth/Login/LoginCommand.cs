// Intranet.Application/Commands/Auth/LoginCommand.cs
using Intranet.Domain;
using MediatR;

namespace Intranet.Application.Features.Auth.Login;

public record LoginCommand(
    string Email,
    string Password
) : IRequest<AuthResult>;