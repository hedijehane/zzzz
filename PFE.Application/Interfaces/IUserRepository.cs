// PFE.Application/Interfaces/IUserRepository.cs
using PFE.Domain.Entities;

namespace PFE.Application.Interfaces;

public interface IUserRepository
{
    // User operations
    Task<User?> GetByIdAsync(int id);
    Task<User?> GetByEmailAsync(string email);
    Task<bool> ExistsByEmailAsync(string email);
    Task<User> CreateAsync(User user);
    Task<User> UpdateAsync(User user);
    Task DeleteAsync(int id);
    Task<User?> GetUserWithDetailsAsync(int userId);

    // Department operations
    Task<bool> DepartmentExistsAsync(int id);
    Task<List<Department>> GetAllDepartmentsAsync();
    Task<Department> AddDepartmentAsync(string departmentName);
    Task<Department?> UpdateDepartmentAsync(int id, string departmentName);
    Task<bool> DeleteDepartmentAsync(int id);
    Task<bool> DepartmentNameExistsAsync(string departmentName);

    // Role operations
    Task<bool> RoleExistsAsync(int id);
    Task<List<Role>> GetAllRolesAsync();
    Task<Role> AddRoleAsync(string roleName);
    Task<Role?> UpdateRoleAsync(int id, string roleName);
    Task<bool> DeleteRoleAsync(int id);
    Task<bool> RoleNameExistsAsync(string roleName);

    // Initialization
    Task StoreResetTokenAsync(string email, string token);
    Task<bool> ValidateResetTokenAsync(string email, string token);
    Task UpdatePasswordAsync(string email, string newPassword);
    Task<List<User>> GetAllUsersWithDetailsAsync();
}
