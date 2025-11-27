using OrderManagement.DTOs;
using OrderManagement.Models;

namespace OrderManagement.Services;

public interface IAuthService
{
    Task<AuthResponseDto?> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto?> LoginAsync(LoginDto loginDto);
    Task<UserDto?> GetUserByIdAsync(string userId);
    Task<IEnumerable<UserDto>> GetAllUsersAsync();
    Task<bool> AssignRoleAsync(string userId, string role);
}
