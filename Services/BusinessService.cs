using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;

namespace NexusFlow.Services
{
    public class BusinessService
    {
        private readonly AppDbContext _context;

        public BusinessService(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task<BusinessResponse> CreateBusinessAsync(BusinessRequest dto, Guid userId)
        {
            var user = await _context.Users.FindAsync(userId);

            if (user == null) throw new Exception("Usuario no encontrado");

            var business = new Business
            {
                Name = dto.Name,
                Address = dto.Address,
                CreatedAt = DateTime.Now
            };

            _context.Business.Add(business);
            await _context.SaveChangesAsync();

            var userBusiness = new UserBusiness
            {
                UserId = userId,
                BusinessId = business.Id,
                Role = BusinessRole.Admin
            };

            _context.UsersBusiness.Add(userBusiness);
            await _context.SaveChangesAsync();

            return new BusinessResponse
            {
                Id = business.Id,
                Name = business.Name,
                Address = business.Address,
            };
        }

        public async Task<BusinessResponse> GetBusinessAsync(Guid userId)
        {
            var userBusiness = await _context.UsersBusiness
                .Include(ub => ub.Business)
                .FirstOrDefaultAsync(ub => ub.UserId == userId);

            if (userBusiness == null) throw new Exception("Negocio no encontrado.");

            return new BusinessResponse
            {
                Id = userBusiness.Business.Id,
                Name = userBusiness.Business.Name,
                Address = userBusiness.Business.Address,
                CreatedAt = userBusiness.Business.CreatedAt
            };
        }

        //public async Task<BusinessResponse> GetBusinessAdminAsync(business)
        //{

        //}

        public async Task<BusinessResponse> UpdateBusinessAsync(Guid userId, BusinessRequest dto)
        {
            var business = await _context.UsersBusiness
                .Where(ub => ub.UserId == userId)
                .Select(ub => ub.Business)
                .FirstOrDefaultAsync();

            if (business == null) throw new Exception("Negocio no encontrado.");

            business.Name = dto.Name;
            business.Address = dto.Address;

            await _context.SaveChangesAsync();

            return new BusinessResponse
            {
                Id = business.Id,
                Name = business.Name,
                Address = business.Address,
                CreatedAt = business.CreatedAt,
            };
        }
    }
}
