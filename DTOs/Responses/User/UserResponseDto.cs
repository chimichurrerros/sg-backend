using BackEnd.Utils;

namespace BackEnd.DTOs.Responses.User;


public class UserResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public string RoleName { get; set; } = null!;
    public DateTime CreatedAt { get; set; } 
}

public class UserWrapperDto
{
    public UserResponseDto User { get; set; } = null!;
}

// Crear todo los ListWrapper con paginacion
public class ListUsersWrapperDto
{
    public List<UserResponseDto> Users { get; set; } = [];
    public Pagination Pagination { get; set; } = null!;
}
