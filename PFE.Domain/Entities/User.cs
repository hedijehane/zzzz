using System.Data;

namespace PFE.Domain.Entities;
public class Role
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation property
    public ICollection<User> Users { get; set; } = new List<User>();
}
public class Department
{
    public int Id { get; set; }
    public required string Name { get; set; }
    public string? Description { get; set; }

    // Navigation property
    public ICollection<User> Users { get; set; } = new List<User>();
}
public class User
{
    public string? ResetToken { get; set; }
    public DateTime? ResetTokenExpiry { get; set; }
    public int Id { get; set; }
    public required string Email { get; set; } 
    public required string PasswordHash { get; set; }
    public required string Name { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public int DepartmentId { get; set; }
    public Department Department { get; set; } = null!;

    public int RoleId { get; set; }
    public Role Role { get; set; } = null!;
    public ICollection<Message> SentMessages { get; set; } = new List<Message>();
    public ICollection<ChatParticipant> ChatParticipants { get; set; } = new List<ChatParticipant>();
}