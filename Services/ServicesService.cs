using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;

namespace NexusFlow.Services
{
    public class ServicesService
    {
        private readonly AppDbContext _context;

        public ServicesService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ServiceResponse> CreateServiceAsync(ServiceRequest dto, Guid userID)
        {
            var userBusiness = await _context.UsersBusiness
                .FirstOrDefaultAsync(ub => ub.UserId == userID && ub.Role == BusinessRole.Admin);
            if (userBusiness == null) throw new Exception("No tienes permisos para crear servicios");

            var service = new Service
            {
                Name = dto.Name,
                Description = dto.Description,
                Price = dto.Price,
                Duration = dto.Duration,
                BusinessId = userBusiness.BusinessId,
            };

            _context.Services.Add(service);
            await _context.SaveChangesAsync();

            return new ServiceResponse
            {
                Id = service.Id,
                Name = service.Name,
            };
        }

        public async Task<List<ServiceResponse>> GetServicesAsync(Guid businessId)
        {
            var services = await _context.Services
                .Where(s => s.BusinessId == businessId)
                .Select(s => new ServiceResponse
                {
                    Id = s.Id,
                    Name = s.Name,
                    Description = s.Description,
                    Price = s.Price,
                    Duration = s.Duration
                })
                .ToListAsync();

            return services;
        }

        public async Task<ServiceResponse> UpdateServiceAsync(Guid serviceId, ServiceRequest dto, Guid userId)
        {
            var service = await _context.Services.FindAsync(serviceId);
            if (service == null) throw new Exception("No se encontró el servicio.");
            var isOwner = await _context.UsersBusiness
                .AnyAsync(ub => ub.UserId == userId &&
                                ub.BusinessId == service.BusinessId &&
                                ub.Role == BusinessRole.Admin);
            if (!isOwner) throw new Exception("No tiene permiso para modificar este servicio.");

            service.Name = dto.Name;
            service.Description = dto.Description;
            service.Price = dto.Price;
            service.Duration = dto.Duration;

            await _context.SaveChangesAsync();

            return new ServiceResponse
            {
                Id = service.Id,
                Name = service.Name,
                Description = service.Description,
                Price = service.Price,
                Duration = service.Duration
            };
        }
    }
}
