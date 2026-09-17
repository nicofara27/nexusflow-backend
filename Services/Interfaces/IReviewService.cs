namespace NexusFlow.Services.Interfaces
{
    public interface IReviewService
    {
        Task<ReviewResponse> CreateAsync(Guid userId, ReviewRequest request);
    }
}
