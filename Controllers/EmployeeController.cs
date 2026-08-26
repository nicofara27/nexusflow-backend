using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusFlow.Models.DTOs.Employee;
using NexusFlow.Models.Entities;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;
using System.Security.Claims;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmployeeController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeeController(IEmployeeService employeeService)
        {
            _employeeService = employeeService;
        }

        [Authorize]
        [HttpPut("schedule")]
        public async Task<IActionResult> UpsertEmployeeSchedule(Guid userBusinessId, [FromBody] EmployeeScheduleRequest dto)
        {
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userClaim == null) return Unauthorized("Token inválido.");

            var adminId = Guid.Parse(userClaim);

            var schedule = await _employeeService.UpsertEmployeeScheduleAsync(userBusinessId, dto, adminId);

            return Ok(schedule);
        }

        [HttpGet("{userBusinessId}/schedule")]
        public async Task<IActionResult> GetEmployeeSchedule(Guid userBusinessId)
        {
            var schedule = await _employeeService.GetEmployeeScheduleAsync(userBusinessId);
            return Ok(schedule);
        }

        [Authorize]
        [HttpGet("{userBusinessId}")]
        public async Task<IActionResult> GetEmployee(Guid userBusinessId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var updaterId = Guid.Parse(userIdClaim);

            var userBusiness = await _employeeService.GetEmployeeAsync(userBusinessId, updaterId);
            return Ok(userBusiness);
        }

        [Authorize]
        [HttpGet("business")]
        public async Task<IActionResult> GetEmployeesByBusinessId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var requesterId = Guid.Parse(userIdClaim);

            var employees = await _employeeService.GetEmployeesByBusinessIdAsync(requesterId);
            return Ok(employees);
        }

        [HttpGet("service/{serviceId}")]
        public async Task<IActionResult> GetEmployeesByServiceId(Guid serviceId)
        {
            var employees = await _employeeService.GetEmployeesByServiceIdAsync(serviceId);
            return Ok(employees);
        }

        [Authorize]
        [HttpPut("{userBusinessId}")]
        public async Task<IActionResult> UpdateEmployee(Guid userBusinessId, [FromBody] UpdateEmployeeRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var updaterId = Guid.Parse(userIdClaim);

            var employee = await _employeeService.UpdateEmployeeAsync(userBusinessId, updaterId, dto);

            return Ok(employee);
        }
    }
}
