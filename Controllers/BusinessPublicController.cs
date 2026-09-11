using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs.Business;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/businesses")]
    public class BusinessPublicController : ControllerBase
    {
        private readonly IBusinessService _businessService;

        public BusinessPublicController(
            IBusinessService businessService)
        {
            _businessService = businessService;
        }

        [HttpGet]
        public async Task<ActionResult<List<BusinessPublicResponse>>> GetBusinesses()
        {
            var businesses = await _businessService.GetAllPublicAsync();

            return Ok(businesses);
        }

        [HttpGet("{businessId}")]
        public async Task<ActionResult<BusinessPublicDetailsResponse>> GetById(
            Guid businessId)
        {
            var business = await _businessService.GetPublicByIdAsync(businessId);

            return Ok(business);
        }
    }
}