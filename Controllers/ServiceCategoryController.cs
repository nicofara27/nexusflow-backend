using Microsoft.AspNetCore.Mvc;
using NexusFlow.Models.DTOs.ServiceCategory;
using NexusFlow.Services.Interfaces;
using System.Security.Claims;

namespace NexusFlow.Controllers
{
    [ApiController]
    [Route("api/service-categories")]
    public class ServiceCategoryController : ControllerBase
    {
        private readonly IServiceCategoryService _serviceCategoryService;

        public ServiceCategoryController(IServiceCategoryService serviceCategoryService)
        {
            _serviceCategoryService = serviceCategoryService;
        }

        [HttpGet]
        public async Task<ActionResult<List<ServiceCategoryResponse>>> GetAll()
        {
            var adminId = GetUserId();

            var categories = await _serviceCategoryService.GetAllAsync(adminId);

            return Ok(categories);
        }

        [HttpPost]
        public async Task<ActionResult<ServiceCategoryResponse>> Create(ServiceCategoryRequest dto)
        {
            var adminId = GetUserId();

            var category = await _serviceCategoryService.CreateAsync(dto, adminId);

            return Ok(category);
        }

        [HttpPut("{categoryId}")]
        public async Task<ActionResult<ServiceCategoryResponse>> Update(Guid categoryId, ServiceCategoryRequest dto)
        {
            var adminId = GetUserId();

            var category = await _serviceCategoryService.UpdateAsync(categoryId, dto, adminId);

            return Ok(category);
        }

        [HttpDelete("{categoryId}")]
        public async Task<IActionResult> Delete(Guid categoryId)
        {
            var adminId = GetUserId();

            await _serviceCategoryService.DeleteAsync(categoryId, adminId);

            return NoContent();
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }
    }
}
