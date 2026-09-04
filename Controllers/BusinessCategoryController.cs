using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs.BusinessCategory;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/business-categories")]
    public class BusinessCategoryController : ControllerBase
    {
        private readonly IBusinessCategroyService _businessCategoryService;

        public BusinessCategoryController(IBusinessCategroyService businessCategoryService)
        {
            _businessCategoryService = businessCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<BusinessCategoryResponse>>> GetAll()
        {
            var categories = await _businessCategoryService.GetAllAsync();
            return Ok(categories);
        }
    }
}
