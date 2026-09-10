using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs.Business;
using NexusFlow.Models.DTOs.BusinessSchedule;
using NexusFlow.Services.Implementations;
using NexusFlow.Services.Interfaces;
using System.Security.Claims;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly IBusinessService _businessService;
        private readonly IServicesService _servicesService;
        private readonly IBusinessScheduleService _businessScheduleService;
        public BusinessController(IBusinessService businessService,IServicesService servicesService, IBusinessScheduleService businessScheduleService)
        {
            _businessService = businessService;
            _servicesService = servicesService;
            _businessScheduleService = businessScheduleService;
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBusiness(BusinessRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var business = await _businessService.CreateBusinessAsync(dto, userId);

            return Ok(new BusinessResponse
            {
                Id = business.Id,
                Name = business.Name,
                Address = business.Address
            });
        }

        [Authorize]
        [HttpGet]
        public async Task<ActionResult<BusinessResponse>> GetBusiness()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userIdClaim == null) return Unauthorized("Token inválido.");

            var userId = Guid.Parse(userIdClaim);

            var business = await _businessService.GetBusinessAsync(userId);

            if (business == null) return NotFound("Negocio no encontrado.");

            return Ok(new BusinessResponse
            {
                Name = business.Name,
                Address = business.Address,
            });
        }

        [Authorize]
        [HttpPut("{businessId}")]
        public async Task<IActionResult> UpdateBusiness(Guid businessId, BusinessRequest dto)
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if(userIdClaim == null) return Unauthorized("Token inválido");

            var userId = Guid.Parse(userIdClaim);

            var business = await _businessService
                .UpdateBusinessAsync(userId, dto);

            return Ok(new BusinessResponse
            {
                Name = business.Name,
                Address = business.Address
            });
        }

        [HttpGet("{businessId}/services")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServices(Guid businessId)
        {
            var services = await _servicesService.GetServicesByBusinessIdAsync(businessId);
            return Ok(services);
        }

        [HttpGet("schedule")]
        public async Task<ActionResult<List<BusinessScheduleResponse>>> GetSchedule()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var schedule = await _businessScheduleService.GetAsync(userId);

            return Ok(schedule);
        }

        [HttpPut("schedule")]
        public async Task<ActionResult<List<BusinessScheduleResponse>>> UpdateSchedule(BusinessScheduleRequest request)
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

            var schedule = await _businessScheduleService.UpdateAsync(userId, request);

            return Ok(schedule);
        }
    }
}
