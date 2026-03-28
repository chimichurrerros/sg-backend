namespace BackEnd.DTOs.Responses.User;


public class UserResponseDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;
    public string LastName { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> PhoneNumbers { get; set; } = [];
    public string RoleName { get; set; } = null!;
}

public class UserWrapperDto
{
    public UserResponseDto User { get; set; } = null!;
}

public class ListUserWrapperDto
{
    public List<UserResponseDto> Users { get; set; } = [];
}
