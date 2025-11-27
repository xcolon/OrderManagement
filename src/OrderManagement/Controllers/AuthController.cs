using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OrderManagement.DTOs;
using OrderManagement.Services;

namespace OrderManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponseDto>> Register(RegisterDto registerDto)
    {
        var result = await _authService.RegisterAsync(registerDto);
        if (result == null)
        {
            return BadRequest("Registration failed. User may already exist or password requirements not met.");
        }
        return Ok(result);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto loginDto)
    {
        var result = await _authService.LoginAsync(loginDto);
        if (result == null)
        {
            return Unauthorized("Invalid email or password.");
        }
        return Ok(result);
    }

    [HttpGet("users")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<IEnumerable<UserDto>>> GetAllUsers()
    {
        var users = await _authService.GetAllUsersAsync();
        return Ok(users);
    }

    [HttpGet("users/{userId}")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<UserDto>> GetUser(string userId)
    {
        var user = await _authService.GetUserByIdAsync(userId);
        if (user == null)
        {
            return NotFound();
        }
        return Ok(user);
    }

    [HttpPost("users/{userId}/roles/{role}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> AssignRole(string userId, string role)
    {
        var result = await _authService.AssignRoleAsync(userId, role);
        if (!result)
        {
            return BadRequest("Failed to assign role.");
        }
        return NoContent();
    }
}
