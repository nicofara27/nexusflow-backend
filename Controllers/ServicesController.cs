using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs.Service;
using NexusFlow.Services.Interfaces;
using System.Security.Claims;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly IServicesService _servicesService;
        public ServicesController(IServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateService([FromBody] ServiceRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var service = await _servicesService.CreateServiceAsync(dto, userId);


            return Ok(new ServiceResponse
            {

            });
        }

        [HttpGet("{serviceId}")]
        [Authorize]
        public async Task<IActionResult> GetService(Guid serviceId)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var service = await _servicesService.GetServiceAsync(serviceId, userId);

            return Ok(service);
        }

        [Authorize]
        [HttpPut("{serviceId}")]
        public async Task<IActionResult> UpdateService(Guid serviceId, [FromBody] UpdateServiceRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var service = await _servicesService.UpdateServiceAsync(serviceId, dto, userId);

            return Ok(service);
        }
    }
}
