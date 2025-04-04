using PFE.Application.Interfaces;
using PFE.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using PFE.Infrastructure.Data;

namespace PFE.Infrastructure.Repositories;
public class UserRepository(ApplicationDbContext context) : IUserRepository
{
    public async Task<User?> GetByEmailAsync(string email)
        => await context.Users.FirstOrDefaultAsync(u => u.Email == email);

    public async Task<User> CreateAsync(User user)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync();
        return user;
    }
}