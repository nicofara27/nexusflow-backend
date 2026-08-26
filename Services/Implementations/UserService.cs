using NexusFlow.Models.DTOs.User;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class UserService : IUserService 
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;
        public UserService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<GetProfileResponse> GetProfileAsync(Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado.");

            return new GetProfileResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task<UpdateProfileResponse> UpdateProfileAsync(Guid userId, UpdateProfileRequest dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado.");

            user.FirstName = dto.FirstName;
            user.LastName = dto.LastName;
            
            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new UpdateProfileResponse
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };
        }
    }
}
