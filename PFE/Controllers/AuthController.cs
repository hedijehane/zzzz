using Microsoft.AspNetCore.Mvc;
using PFE.Application.UseCases.Auth;
using PFE.Application.DTOs;
using PFE.Application.Interfaces;

namespace PFE.Web.Controllers;

public class AuthController : Controller
{
    private readonly RegisterUser _registerUser;
    private readonly LoginUser _loginUser;
    private readonly IUserRepository _userRepository;
    private readonly ResetPassword _resetPassword;
    private readonly ForgotPassword _forgotPassword; // Added this

    public AuthController(
        RegisterUser registerUser,
        LoginUser loginUser,
        IUserRepository userRepository,
        ResetPassword resetPassword,
        ForgotPassword forgotPassword) // Added this
    {
        _registerUser = registerUser;
        _loginUser = loginUser;
        _userRepository = userRepository;
        _resetPassword = resetPassword;
        _forgotPassword = forgotPassword; // Added this
    }
    [HttpGet]
    public async Task<IActionResult> Register()
    {
        // Get all departments and filter out the "System" department
        var departments = await _userRepository.GetAllDepartmentsAsync();
        var filteredDepartments = departments.Where(d => d.Name != "System").ToList();

        // Get all roles and filter to only include "Employee" role
        var roles = await _userRepository.GetAllRolesAsync();
        var employeeRole = roles.Where(r => r.Name == "Employee").ToList();

        ViewBag.Departments = filteredDepartments;
        ViewBag.Roles = employeeRole;

        return View(new RegisterDto());
    }


    [HttpPost]
    public async Task<IActionResult> Register(RegisterDto model)
    {
        if (!ModelState.IsValid)
        {
            // Get all departments and filter out the "System" department
            var departments = await _userRepository.GetAllDepartmentsAsync();
            var filteredDepartments = departments.Where(d => d.Name != "System").ToList();

            // Get all roles and filter to only include "Employee" role
            var roles = await _userRepository.GetAllRolesAsync();
            var employeeRole = roles.Where(r => r.Name == "Employee").ToList();

            ViewBag.Departments = filteredDepartments;
            ViewBag.Roles = employeeRole;

            return View(model);
        }

        try
        {
            await _registerUser.Execute(model);
            return RedirectToAction("Login", new { registered = true });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);

            // Filter roles and departments again in case of error
            var departments = await _userRepository.GetAllDepartmentsAsync();
            var filteredDepartments = departments.Where(d => d.Name != "System").ToList();
            var roles = await _userRepository.GetAllRolesAsync();
            var employeeRole = roles.Where(r => r.Name == "Employee").ToList();

            ViewBag.Departments = filteredDepartments;
            ViewBag.Roles = employeeRole;

            return View(model);
        }
    }


    [HttpGet]
    public IActionResult Login(bool? registered = false, bool? resetSuccess = false)
    {
        if (registered == true)
        {
            ViewBag.RegistrationSuccess = "Registration successful! Please login.";
        }
        if (resetSuccess == true)
        {
            ViewBag.ResetSuccess = "Password reset successful! Please login with your new password.";
        }
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // First authenticate the user
            await _loginUser.Execute(model);

            // Then get the user with their role using existing repository methods
            var user = await _userRepository.GetByEmailAsync(model.Email);
            if (user == null)
            {
                ModelState.AddModelError("", "User not found");
                return View(model);
            }

            // Get the role name using existing GetAllRolesAsync
            var allRoles = await _userRepository.GetAllRolesAsync();
            var userRole = allRoles.FirstOrDefault(r => r.Id == user.RoleId)?.Name;

            // Redirect based on role
            if (userRole == "Admin")
            {
                return RedirectToAction("Dashboard", "Admin");
            }

            return RedirectToAction("Index", "Home");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public async Task<IActionResult> ResetPassword(string email, string token)
    {
        var user = await _userRepository.GetByEmailAsync(email);

        // Check if user exists and if token is valid
        if (user == null || user.ResetToken != token || user.ResetTokenExpiry == null || user.ResetTokenExpiry < DateTime.UtcNow)
        {
            // Redirect to the TokenExpired view in Views/Auth
            return View("TokenExpired");
        }

        return View(new ResetPasswordDto(email, token, "", ""));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ResetPassword(ResetPasswordDto model)
    {
        // Manually check if NewPassword and ConfirmPassword match
        if (model.NewPassword != model.ConfirmPassword)
        {
            ModelState.AddModelError("ConfirmPassword", "Passwords do not match.");
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Execute password reset logic
            await _resetPassword.Execute(model);
            return RedirectToAction("Login", new { resetSuccess = true });
        }
        catch (Exception ex)
        {
            ModelState.AddModelError("", ex.Message);  // Add general error
            return View(model);
        }
    }


    [HttpGet]
    public IActionResult ForgotPassword()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordDto model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            // Check if the email exists in the database
            var user = await _userRepository.GetByEmailAsync(model.Email);

            // If the user is not found, add an error message to ModelState
            if (user == null)
            {
                ModelState.AddModelError("", "No account found with that email.");
                return View(model);  // Return the view with error
            }

            // If the email exists, proceed with the password reset process
            await _forgotPassword.Execute(model);

            // Redirect to the confirmation page
            return RedirectToAction("ForgotPasswordConfirmation");
        }
        catch (Exception ex)
        {
            // Handle any other exceptions and display the message
            ModelState.AddModelError("", ex.Message);
            return View(model);
        }
    }

    [HttpGet]
    public IActionResult ForgotPasswordConfirmation()
    {
        return View();
    }
}