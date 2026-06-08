using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs;
using NexusFlow.Services;

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
            try
            {
                if (dto == null) return BadRequest(new { mensaje = "Los datos son requeridos" });

                await _authService.RegisterAsync(dto);
                return StatusCode(201, new { mensaje = "Usuario registrado correctamente " });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    mensaje = ex.Message
                });
            }
        }
        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginRequest dto)
        {
            try
            {
                if (dto == null) return BadRequest(new
                {
                    mensaje = "Los datos son requeridos"
                });

                var response = await _authService.Login(dto);
                return Ok(response);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new
                {
                    mensaje = ex.Message
                });
            }
            catch (Exception ex)
            {
                return Unauthorized(new
                {
                    mensaje = ex.Message
                });
            }
        }
    }
}
