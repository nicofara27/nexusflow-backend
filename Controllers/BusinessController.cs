using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs;
using NexusFlow.Services;
using System.Security.Claims;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BusinessController : ControllerBase
    {
        private readonly BusinessService _businessService;
        public BusinessController(BusinessService businessService)
        {
            _businessService = businessService;
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
                Id = business.Id,
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
                Id = businessId,
                Name = business.Name,
                Address = business.Address
            });
        }
    }
}
