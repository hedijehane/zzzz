// Intranet.Domain/AuthResult.cs
public class AuthResult
{
    public bool Success { get; init; }
    public User? User { get; init; }
    public string? ErrorMessage { get; init; }
    public string? Token { get; init; } // Add if implementing JWT

    public static AuthResult Success(User user) => new()
    {
        Success = true,
        User = user
    };

    public static AuthResult Failure(string error) => new()
    {
        Success = false,
        ErrorMessage = error
    };
}