// Intranet.Application/Commands/Auth/RegisterCommand.cs
using Intranet.Domain;
using MediatR;

namespace Intranet.Application.Features.Auth.Register;

public record RegisterCommand(
    string Name,
    string Email,
    string Password
) : IRequest<AuthResult>;