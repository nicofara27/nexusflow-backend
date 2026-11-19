using NexusFlow.Exceptions;
using NexusFlow.Models.DTOs.User;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;
using System.Text.RegularExpressions;

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
            if (user == null) throw new NotFoundException("Usuario no encontrado.");

            return new GetProfileResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }


        public async Task<UpdateProfileResponse> UpdateProfileAsync(
            Guid userId,
            UpdateProfileRequest dto)
        {
            var user = await _userRepository.GetByIdAsync(userId);

            if (user == null)
                throw new NotFoundException("Usuario no encontrado.");

            var firstName = ValidateName(dto.FirstName, "El nombre");
            var lastName = ValidateName(dto.LastName, "El apellido");

            user.FirstName = firstName;
            user.LastName = lastName;

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();

            return new UpdateProfileResponse
            {
                FirstName = user.FirstName,
                LastName = user.LastName
            };
        }


        private static string ValidateName(string? value, string field)
        {
            var normalized = value?.Trim() ?? string.Empty;

            if (string.IsNullOrWhiteSpace(normalized))
                throw new ValidationException(
                    $"{field} es obligatorio.");

            if (normalized.Length < 2 || normalized.Length > 50)
                throw new ValidationException(
                    $"{field} debe tener entre 2 y 50 caracteres.");

            if (!Regex.IsMatch(
                normalized,
                @"\A\p{L}+(?:[ '-]\p{L}+)*\z"))
            {
                throw new ValidationException(
                    $"{field} contiene caracteres inválidos.");
            }

            return normalized;
        }

    }
}
