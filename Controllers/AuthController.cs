using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs.Auth;
using NexusFlow.Services.Interfaces;
using System.Security.Claims;


namespace NexusFlow.Controllers
{
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
        public async Task<IActionResult> Register(RegisterRequest dto)
        {
            var result = await _authService.RegisterAsync(dto);

            SetRefreshTokenCookie(result);

            return Ok(new LoginResponse
            {
                Token = result.Token,
                FirstName = result.FirstName,
                LastName = result.LastName,
                Email = result.Email
            });
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest dto)
        {
            var result = await _authService.LoginAsync(dto);

            SetRefreshTokenCookie(result);

            return Ok(new LoginResponse
            {
                Token = result.Token,
                FirstName = result.FirstName,
                LastName = result.LastName,
                Email = result.Email
            });
        }

        [Authorize]
        [HttpPost("register-employee")]
        public async Task<IActionResult> RegisterEmployee(RegisterRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var adminUserId = Guid.Parse(userIdClaim);

            var employee = await _authService.RegisterEmployeeAsync(dto, adminUserId);

            return CreatedAtAction(nameof(RegisterEmployee), new { id = employee.Id }, employee);
        }
        [Authorize]
        [HttpPost("register-admin")]
        public async Task<IActionResult> RegisterAdmin(RegisterAdminRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var adminUserId = Guid.Parse(userIdClaim);

            var admin = await _authService.RegisterAdminAsync(dto, adminUserId);

            return CreatedAtAction(nameof(RegisterAdmin), new { id = admin.Id }, admin);
        }
        [Authorize]
        [HttpPut("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            await _authService.ChangePasswordAsync(dto, userId);
            return NoContent();
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {

            var refreshToken = Request.Cookies["refreshToken"];
            if (string.IsNullOrEmpty(refreshToken)) return Unauthorized("Refresh token no encontrado.");

            try
            {
                var result = await _authService.RefreshAsync(refreshToken);

                SetRefreshTokenCookie(result);

                return Ok(new LoginResponse
                {
                    Token = result.Token,
                    FirstName = result.FirstName,
                    LastName = result.LastName,
                    Email = result.Email
                });

            }
            catch (UnauthorizedAccessException ex)
            {
                Response.Cookies.Delete("refreshToken");

                return Unauthorized(ex.Message);
            }


        }

        [HttpPost("logout")]
        public async Task<IActionResult> Logout()
        {
            var refreshToken = Request.Cookies["refreshToken"];

            if (!string.IsNullOrEmpty(refreshToken))
            {
                await _authService.LogoutAsync(refreshToken);
            }

            Response.Cookies.Delete("refreshToken");

            return NoContent();
        }

        private void SetRefreshTokenCookie(AuthResult result)
        {
            Response.Cookies.Append(
                "refreshToken",
                result.RefreshToken,
                new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                    Expires = result.RefreshTokenExpiry
                }
            );
        }
    }
}
