// PFE.Application/DTOs/ViewDtos.cs
namespace PFE.Application.DTOs;

public record DepartmentViewDto(int Id, string Name);
public record RoleViewDto(int Id, string Name);
    

    public record UserViewDto
    {
        public int Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Email { get; init; } = string.Empty;
        public int DepartmentId { get; init; }
        public int RoleId { get; init; }
        public DepartmentViewDto Department { get; init; } = new DepartmentViewDto(0, string.Empty);
        public RoleViewDto Role { get; init; } = new RoleViewDto(0, string.Empty);
    }

