using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PFE.Application.DTOs;
using PFE.Application.Interfaces;
using PFE.Domain.Entities;

public class AdminController : Controller
{
    private readonly IUserRepository _userRepository;

    public AdminController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public IActionResult Dashboard() => View();

    // List all users
    public async Task<IActionResult> ManageUsers()
    {
        var users = await _userRepository.GetAllUsersWithDetailsAsync();
        var departments = await _userRepository.GetAllDepartmentsAsync();
        var roles = await _userRepository.GetAllRolesAsync();

        // Map users to UserViewDto
        var userDtos = users.Select(user => new UserViewDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            DepartmentId = user.DepartmentId,
            RoleId = user.RoleId,
            Department = new DepartmentViewDto(user.Department.Id, user.Department.Name),
            Role = new RoleViewDto(user.Role.Id, user.Role.Name)
        }).ToList();

        // Map roles and departments to their respective DTOs
        var roleDtos = roles.Select(role => new RoleViewDto(role.Id, role.Name)).ToList();
        var departmentDtos = departments.Select(department => new DepartmentViewDto(department.Id, department.Name)).ToList();

        // Pass departments, roles and userDtos to the view
        ViewBag.Departments = departmentDtos;
        ViewBag.Roles = roleDtos;

        return View(userDtos);
    }

    // GET: Edit user form
    public async Task<IActionResult> EditUser(int id)
    {
        var user = await _userRepository.GetUserWithDetailsAsync(id);
        var departments = await _userRepository.GetAllDepartmentsAsync();
        var roles = await _userRepository.GetAllRolesAsync();

        // Map user to UserViewDto
        var userDto = new UserViewDto
        {
            Id = user.Id,
            Name = user.Name,
            Email = user.Email,
            DepartmentId = user.DepartmentId,
            RoleId = user.RoleId,
            Department = new DepartmentViewDto(user.Department.Id, user.Department.Name),
            Role = new RoleViewDto(user.Role.Id, user.Role.Name)
        };

        // Map roles and departments to their respective DTOs
        var roleDtos = roles.Select(role => new RoleViewDto(role.Id, role.Name)).ToList();
        var departmentDtos = departments.Select(department => new DepartmentViewDto(department.Id, department.Name)).ToList();

        // Pass departments, roles and userDto to the view
        ViewBag.Departments = departmentDtos;
        ViewBag.Roles = roleDtos;

        return View(userDto);
    }

    // POST: Update user
    [HttpPost]
    public async Task<IActionResult> EditUser(UserViewDto userDto)
    {
        if (!ModelState.IsValid)
        {
            return View(userDto);
        }

        var existingUser = await _userRepository.GetByIdAsync(userDto.Id);
        if (existingUser == null)
        {
            TempData["Error"] = "User not found.";
            return RedirectToAction(nameof(ManageUsers));
        }

        existingUser.Name = userDto.Name;
        existingUser.Email = userDto.Email;
        existingUser.DepartmentId = userDto.DepartmentId;
        existingUser.RoleId = userDto.RoleId;

        await _userRepository.UpdateAsync(existingUser);

        TempData["Success"] = "User updated successfully.";
        return RedirectToAction(nameof(ManageUsers));
    }

    // POST: Delete user
    [HttpPost]
    public async Task<IActionResult> DeleteUser(int id)
    {
        await _userRepository.DeleteAsync(id);
        TempData["Success"] = "User deleted successfully.";
        return RedirectToAction(nameof(ManageUsers));
    }

    // View all roles
    [HttpGet]
    public async Task<IActionResult> ManageRoles()
    {
        var roles = await _userRepository.GetAllRolesAsync();
        var roleDtos = roles.Select(role => new RoleViewDto(role.Id, role.Name)).ToList();
        return View(roleDtos);
    }

    // GET: Add new role form
    [HttpGet]
    public IActionResult AddRole()
    {
        return View();
    }

    // POST: Add new role
    [HttpPost]
    public async Task<IActionResult> AddRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["Error"] = "Role name is required.";
            return RedirectToAction(nameof(ManageRoles));
        }

        if (await _userRepository.RoleNameExistsAsync(roleName))
        {
            TempData["Error"] = "Role already exists.";
            return RedirectToAction(nameof(ManageRoles));
        }

        await _userRepository.AddRoleAsync(roleName);
        TempData["Success"] = "Role added successfully.";
        return RedirectToAction(nameof(ManageRoles));
    }

    // GET: Edit role form
    [HttpGet]
    public async Task<IActionResult> EditRole(int id)
    {
        var role = (await _userRepository.GetAllRolesAsync()).FirstOrDefault(r => r.Id == id);
        if (role == null)
        {
            TempData["Error"] = "Role not found.";
            return RedirectToAction(nameof(ManageRoles));
        }

        var roleDto = new RoleViewDto(role.Id, role.Name);
        return View(roleDto);
    }

    // POST: Update role
    [HttpPost]
    public async Task<IActionResult> UpdateRole(int id, string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
        {
            TempData["Error"] = "Role name is required.";
            return RedirectToAction(nameof(ManageRoles));
        }

        var roles = await _userRepository.GetAllRolesAsync();
        if (roles.Any(r => r.Name.ToLower() == roleName.ToLower() && r.Id != id))
        {
            TempData["Error"] = "Another role with the same name already exists.";
            return RedirectToAction(nameof(ManageRoles));
        }

        var updated = await _userRepository.UpdateRoleAsync(id, roleName);
        if (updated == null)
        {
            TempData["Error"] = "Role not found.";
            return RedirectToAction(nameof(ManageRoles));
        }

        TempData["Success"] = "Role updated successfully.";
        return RedirectToAction(nameof(ManageRoles));
    }

    // POST: Delete role
    [HttpPost]
    public async Task<IActionResult> DeleteRole(int id)
    {
        var result = await _userRepository.DeleteRoleAsync(id);
        TempData[result ? "Success" : "Error"] = result
            ? "Role deleted successfully."
            : "Failed to delete role.";
        return RedirectToAction(nameof(ManageRoles));
    }

    // View all departments
    public async Task<IActionResult> ManageDepartments()
    {
        var departments = await _userRepository.GetAllDepartmentsAsync();
        var departmentDtos = departments.Select(department => new DepartmentViewDto(department.Id, department.Name)).ToList();
        return View(departmentDtos);
    }

    // POST: Add department
    [HttpPost]
    public async Task<IActionResult> AddDepartment(string departmentName)
    {
        if (string.IsNullOrWhiteSpace(departmentName))
        {
            TempData["Error"] = "Department name is required.";
            return RedirectToAction(nameof(ManageDepartments));
        }

        if (await _userRepository.DepartmentNameExistsAsync(departmentName))
        {
            TempData["Error"] = "Department already exists.";
            return RedirectToAction(nameof(ManageDepartments));
        }

        await _userRepository.AddDepartmentAsync(departmentName);
        TempData["Success"] = "Department added successfully.";
        return RedirectToAction(nameof(ManageDepartments));
    }

    // POST: Update department
    [HttpPost]
    public async Task<IActionResult> UpdateDepartment(int id, string departmentName)
    {
        if (string.IsNullOrWhiteSpace(departmentName))
        {
            TempData["Error"] = "Department name is required.";
            return RedirectToAction(nameof(ManageDepartments));
        }

        var departments = await _userRepository.GetAllDepartmentsAsync();
        if (departments.Any(d => d.Name.ToLower() == departmentName.ToLower() && d.Id != id))
        {
            TempData["Error"] = "Another department with the same name already exists.";
            return RedirectToAction(nameof(ManageDepartments));
        }

        var updated = await _userRepository.UpdateDepartmentAsync(id, departmentName);
        if (updated == null)
        {
            TempData["Error"] = "Department not found.";
            return RedirectToAction(nameof(ManageDepartments));
        }

        TempData["Success"] = "Department updated successfully.";
        return RedirectToAction(nameof(ManageDepartments));
    }

    // POST: Delete department
    [HttpPost]
    public async Task<IActionResult> DeleteDepartment(int id)
    {
        var success = await _userRepository.DeleteDepartmentAsync(id);
        TempData[success ? "Success" : "Error"] = success
            ? "Department deleted successfully."
            : "Failed to delete department.";
        return RedirectToAction(nameof(ManageDepartments));
    }
}
