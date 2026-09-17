namespace NexusFlow.Repositories.Interfaces
{
    public interface IReviewRepository
    {
        Task<Review?> GetByAppointmentIdAsync(Guid appointmentId);
        Task AddAsync(Review review);
    }
}