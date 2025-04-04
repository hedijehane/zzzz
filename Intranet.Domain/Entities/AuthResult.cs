// Intranet.Domain/AuthResult.cs
using Intranet.Domain.Entities;

namespace Intranet.Domain;

public class AuthResult
{
    public bool Success { get; init; }
    public User? User { get; init; }
    public string? ErrorMessage { get; init; }

    public static AuthResult Success(User user) => new() { Success = true, User = user };
    public static AuthResult Failure(string error) => new() { Success = false, ErrorMessage = error };
}