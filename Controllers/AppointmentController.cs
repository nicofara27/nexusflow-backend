using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs.Appointment;
using NexusFlow.Models.Enums;
using NexusFlow.Services.Implementations;
using NexusFlow.Services.Interfaces;
using System.Security.Claims;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AppointmentController : ControllerBase
    {
        private readonly IAppointmentService _appointmentService;
        public AppointmentController(IAppointmentService appointmentService)
        {
            _appointmentService = appointmentService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateAppointment(AppointmentRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var appointment = await _appointmentService.CreateAppointmentAsync(userId, dto);

            return CreatedAtAction(nameof(CreateAppointment), new { id = appointment.Id }, appointment);
        }

        [Authorize]
        [HttpPut("{appointmentId}")]
        public async Task<IActionResult> UpdateAppointmentStatus(Guid appointmentId, [FromBody] AppointmentStatus newStatus)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token invalido.");

            var userId = Guid.Parse(userIdClaim);

            var appointment = await _appointmentService.UpdateAppointmentStatusAsync(userId, appointmentId, newStatus);

            return Ok(appointment);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAppointments([FromQuery] Guid? employeeId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) throw new Exception("Token invalido.");

            var userId = Guid.Parse(userIdClaim);

            var appointments = await _appointmentService.GetAppointmentsAsync(userId, employeeId);

            return Ok(appointments);
        }

        [HttpGet("{userBusinessId}/availability")]
        public async Task<IActionResult> GetEmployeeAvailabilityAsync(
            Guid userBusinessId,
            [FromQuery] Guid serviceId,
            [FromQuery] DateOnly date)
        {
            var avilableTurns = await _appointmentService.GetEmployeeAvailabilityAsync(userBusinessId, serviceId, date);
            return Ok(avilableTurns);
        }
    }
}
