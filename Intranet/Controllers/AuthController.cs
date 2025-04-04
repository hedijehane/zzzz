// Intranet.Web/Controllers/AuthController.cs
using Intranet.Application.Features.Auth.Login;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace Intranet.Web.Controllers;

[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var command = new LoginCommand(request.Email, request.Password);
        var result = await _mediator.Send(command);

        return result.Success
            ? Ok(result.User)
            : BadRequest(result.ErrorMessage);
    }
}

// Request DTO (add to your project)
public record LoginRequest(string Email, string Password);