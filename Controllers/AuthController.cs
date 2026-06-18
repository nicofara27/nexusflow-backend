using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs;
using NexusFlow.Services;
using System.Security.Claims;


namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly AuthService _authService;

        public AuthController(AuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterRequest dto)
        {
            var user = await _authService.RegisterAsync(dto);

            return CreatedAtAction(nameof(Register), new { id = user.Id }, user);

        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest dto)
        {
            var response = await _authService.LoginAsync(dto);
            return Ok(response);
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> RegisterEmployee(RegisterRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var adminUserId = Guid.Parse(userIdClaim);

            var employee = await _authService.RegisterEmployeeAsync(dto, adminUserId);

            return CreatedAtAction(nameof(RegisterEmployee), new { id = employee.Id }, employee);
        }
    }
}
