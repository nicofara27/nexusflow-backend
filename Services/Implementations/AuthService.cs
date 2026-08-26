using NexusFlow.Models.DTOs.Auth;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;
using NexusFlow.Repositories.Interfaces;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{

    public class AuthService : IAuthService
    {
        private readonly IBusinessRepository _businessRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUserBusinessRepository _userBusinessRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IPasswordService _passwordService;
        private readonly IJwtService _jwtService;

        public AuthService(IBusinessRepository businessRepository,
            IUserRepository userRepository,
            IUserBusinessRepository userBusinessRepository,
            IUnitOfWork unitOfWork,
            IPasswordService passwordService,
            IJwtService jwtService)
        {
            _businessRepository = businessRepository;
            _userRepository = userRepository;
            _userBusinessRepository = userBusinessRepository;
            _unitOfWork = unitOfWork;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<AuthResult> RegisterAsync(RegisterRequest dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exists = await _userRepository.ExistsByEmailAsync(normalizedEmail);

            if (exists) throw new ArgumentException("El email ya esta registrado.");

            var passwordHash = _passwordService.hashPassword(dto.Password);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = normalizedEmail,
                PasswordHash = passwordHash,
            };

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiry;

            _userRepository.Add(user);
            await _unitOfWork.SaveChangesAsync();

            return new AuthResult
            {
                Token = token,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task<AuthResult> LoginAsync(LoginRequest dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var user = await _userRepository.GetByEmailAsync(normalizedEmail);
            if (user == null) throw new ArgumentException("Usuario no encontrado");

            var isValid = _passwordService.verifyPassword(dto.Password, user.PasswordHash);
            if (!isValid) throw new ArgumentException("Credenciales invalidas");

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiry = refreshTokenExpiry;
            await _unitOfWork.SaveChangesAsync();

            return new AuthResult
            {
                Token = token,
                RefreshToken = refreshToken,
                RefreshTokenExpiry = refreshTokenExpiry,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task<RegisterResponse> RegisterEmployeeAsync(RegisterRequest dto, Guid adminId)
        {
            var adminUserBusiness = await _userBusinessRepository.GetByUserIdAsync(adminId, BusinessRole.Admin);
            if (adminUserBusiness == null) throw new Exception("No tienes permisos para crear servicios");

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exists = await _userRepository.ExistsByEmailAsync(normalizedEmail);

            if (exists) throw new Exception("El email ya esta registrado.");

            var passwordHash = _passwordService.hashPassword(dto.Password);

            RegisterResponse response = null!;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var employee = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                };

                _userRepository.Add(employee);
                await _unitOfWork.SaveChangesAsync();

                var userBusiness = new UserBusiness
                {
                    UserId = employee.Id,
                    BusinessId = adminUserBusiness.BusinessId,
                    Role = BusinessRole.Employee
                };

                _userBusinessRepository.Add(userBusiness);
                await _unitOfWork.SaveChangesAsync();

                response = new RegisterResponse
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email
                };
            });

            return response;
        }

        public async Task<RegisterResponse> RegisterAdminAsync(RegisterAdminRequest dto, Guid superAdminId)
        {
            var SuperAdminUserBusiness = await _userRepository.IsSuperAdminAsync(superAdminId);
            if (!SuperAdminUserBusiness) throw new Exception("No tienes permisos para crear servicios.");

            var business = await _businessRepository.GetByIdAsync(dto.BusinessId);
            if (business == null) throw new Exception("Negocio no encontrado.");

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exists = await _userRepository.ExistsByEmailAsync(normalizedEmail);

            if (exists) throw new Exception("El email ya esta registrado.");

            var passwordHash = _passwordService.hashPassword(dto.Password);

            RegisterResponse response = null!;

            await _unitOfWork.ExecuteInTransactionAsync(async () =>
            {
                var admin = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                };

                _userRepository.Add(admin);
                await _unitOfWork.SaveChangesAsync();

                var userBusiness = new UserBusiness
                {
                    UserId = admin.Id,
                    BusinessId = dto.BusinessId,
                    Role = BusinessRole.Admin,
                };

                _userBusinessRepository.Add(userBusiness);
                await _unitOfWork.SaveChangesAsync();

                response = new RegisterResponse
                {
                    Id = admin.Id,
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email
                };
            });

            return response;
        }

        public async Task ChangePasswordAsync(ChangePasswordRequest dto, Guid userId)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) throw new Exception("Usuario no encontrado.");

            var isPasswordValid = _passwordService.verifyPassword(dto.CurrentPassword, user.PasswordHash);
            if (!isPasswordValid) throw new Exception("La contraseña actual es incorrecta.");

            user.PasswordHash = _passwordService.hashPassword(dto.NewPassword); 

            _userRepository.Update(user);
            await _unitOfWork.SaveChangesAsync();
        }   

        public async Task<AuthResult> RefreshAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if (user == null) throw new UnauthorizedAccessException("Refresh token inválido.");

            if (user.RefreshTokenExpiry == null ||
               user.RefreshTokenExpiry <= DateTime.UtcNow)
            {
                user.RefreshToken = null;
                user.RefreshTokenExpiry = null;

                await _unitOfWork.SaveChangesAsync();

                throw new UnauthorizedAccessException("Refresh token expirado.");
            }

            var token = _jwtService.GenerateToken(user);
            var newRefreshToken = _jwtService.GenerateRefreshToken();
            var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(30);

            user.RefreshToken = newRefreshToken;
            user.RefreshTokenExpiry = newRefreshTokenExpiry;

            await _unitOfWork.SaveChangesAsync();

            return new AuthResult
            {
                Token = token,
                RefreshToken = newRefreshToken,
                RefreshTokenExpiry = newRefreshTokenExpiry,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task LogoutAsync(string refreshToken)
        {
            var user = await _userRepository.GetByRefreshTokenAsync(refreshToken);
            if(user == null) return;

            user.RefreshToken = null;
            user.RefreshTokenExpiry = null;

            await _unitOfWork.SaveChangesAsync();
        }
    }
}
