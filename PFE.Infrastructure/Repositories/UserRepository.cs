// PFE.Infrastructure/Repositories/UserRepository.cs
using PFE.Application.Interfaces;
using PFE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PFE.Infrastructure.Data;

namespace PFE.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly ApplicationDbContext _context;

    public UserRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    // User operations
    public async Task<User?> GetByIdAsync(int id)
        => await _context.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email)
        => await _context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<bool> ExistsByEmailAsync(string email)
        => await _context.Users.AnyAsync(u => u.Email == email);

    public async Task<User> CreateAsync(User user)
    {
        if (!await DepartmentExistsAsync(user.DepartmentId))
            throw new InvalidOperationException("Invalid department");

        if (!await RoleExistsAsync(user.RoleId))
            throw new InvalidOperationException("Invalid role");

        await _context.Users.AddAsync(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<User> UpdateAsync(User user)
    {
        _context.Users.Update(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task DeleteAsync(int id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            _context.Users.Remove(user);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<User?> GetUserWithDetailsAsync(int userId)
        => await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Id == userId);

    // Password reset operations
    public async Task StoreResetTokenAsync(string email, string token)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.ResetToken = token;
            user.ResetTokenExpiry = DateTime.UtcNow.AddHours(5); // 5 hour expiration
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ValidateResetTokenAsync(string email, string token)
    {
        return await _context.Users
            .AnyAsync(u => u.Email == email
                        && u.ResetToken == token
                        && u.ResetTokenExpiry > DateTime.UtcNow);
    }

    public async Task UpdatePasswordAsync(string email, string newPassword)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user != null)
        {
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;
            await _context.SaveChangesAsync();
        }
    }

    // Department operations
    public async Task<bool> DepartmentExistsAsync(int id)
        => await _context.Departments.AnyAsync(d => d.Id == id);

    public async Task<List<Department>> GetAllDepartmentsAsync()
        => await _context.Departments.ToListAsync();

    // Role operations
    public async Task<bool> RoleExistsAsync(int id)
        => await _context.Roles.AnyAsync(r => r.Id == id);

    public async Task<List<Role>> GetAllRolesAsync()
        => await _context.Roles.ToListAsync();

    // Initialization
    public async Task EnsureDefaultDepartmentExists()
    {
        if (!await _context.Departments.AnyAsync())
        {
            await _context.Departments.AddAsync(new Department { Name = "Default" });
            await _context.SaveChangesAsync();
        }
    }
    public async Task<List<User>> GetAllUsersWithDetailsAsync()
    {
        return await _context.Users
            .Include(u => u.Department)
            .Include(u => u.Role)
            .ToListAsync();
    }
    public async Task<bool> RoleNameExistsAsync(string roleName)
    {
        return await _context.Roles.AnyAsync(r => r.Name.ToLower() == roleName.ToLower());
    }

    

    public async Task<Role> AddRoleAsync(string roleName)
    {
        var role = new Role { Name = roleName };
        _context.Roles.Add(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<Role?> UpdateRoleAsync(int id, string roleName)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role == null)
            return null;

        role.Name = roleName;
        _context.Roles.Update(role);
        await _context.SaveChangesAsync();
        return role;
    }

    public async Task<bool> DeleteRoleAsync(int id)
    {
        var role = await _context.Roles.FindAsync(id);
        if (role != null)
        {
            _context.Roles.Remove(role);
            await _context.SaveChangesAsync();
            return true;
        }
        return false;
    }
    public async Task<Department> AddDepartmentAsync(string departmentName)
    {
        var department = new Department { Name = departmentName };
        _context.Departments.Add(department);
        await _context.SaveChangesAsync();
        return department;
    }

    public async Task<Department?> UpdateDepartmentAsync(int id, string departmentName)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return null;

        department.Name = departmentName;
        await _context.SaveChangesAsync();  
        return department;
    }

    public async Task<bool> DeleteDepartmentAsync(int id)
    {
        var department = await _context.Departments.FindAsync(id);
        if (department == null) return false;

        _context.Departments.Remove(department);
        await _context.SaveChangesAsync();
        return true;
    }
    public async Task<bool> DepartmentNameExistsAsync(string departmentName)
    {
        return await _context.Departments.AnyAsync(d => d.Name.ToLower() == departmentName.ToLower());
    }

}