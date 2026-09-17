using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Repositories.Interfaces;

namespace NexusFlow.Repositories.Implementations
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly AppDbContext _context;

        public ReviewRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Review?> GetByAppointmentIdAsync(Guid appointmentId)
        {
            return await _context.Reviews.FirstOrDefaultAsync(r => r.AppointmentId == appointmentId);
        }

        public async Task AddAsync(Review review)
        {
            await _context.Reviews.AddAsync(review);
        }
    }
}
