using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs;
using NexusFlow.Services;
using System.Security.Claims;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("/api/[controller]")]
    public class ServicesController : ControllerBase
    {
        private readonly ServicesService _servicesService;
        public ServicesController(ServicesService servicesService)
        {
            _servicesService = servicesService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateService(ServiceRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine(userIdClaim);
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var service = await _servicesService.CreateServiceAsync(dto, userId);

            return Ok(service);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetServices([FromQuery] Guid businessId)
        {
            var services = await _servicesService.GetServicesAsync(businessId);
            return Ok(services);
        }

        [Authorize]
        [HttpPut("{serviceId}")]
        public async Task<IActionResult> UpdateService(Guid serviceId, ServiceRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var service = await _servicesService.UpdateServiceAsync(serviceId, dto, userId);

            return Ok(service);
        }
    }
}
