using NexusFlow.Models.DTOs.User;

namespace NexusFlow.Services.Interfaces
{
    public interface IUserService
    {
        Task<GetProfileResponse> GetProfileAsync(Guid userId);
        Task<UpdateProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest dto);
    }
}
