using NexusFlow.Data;
using NexusFlow.Models.DTOs.BusinessCategory;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class BusinessCategoryService : IBusinessCategroyService
    {
        private readonly IBusinessCategoryRepository _businessCategoryRepository;

        public BusinessCategoryService(IBusinessCategoryRepository businessCategoryRepository)
        {
            _businessCategoryRepository = businessCategoryRepository;
        }

        public async Task<List<BusinessCategoryResponse>> GetAllAsync()
        {
            var categories = await _businessCategoryRepository.GetAllAsync();
            return categories.Select(c => new BusinessCategoryResponse
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug
            }).ToList();
        }
    }
}
