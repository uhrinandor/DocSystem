using UserContract = DocSystem.TypeContracts.User;

namespace DocSystem.Dtos;

public class LoginResultDto
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public UserContract User { get; set; } = new();
}
