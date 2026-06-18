using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;
using NexusFlow.Models.Enums;

namespace NexusFlow.Services
{

    public class AuthService
    {
        private readonly AppDbContext _context;
        private readonly PasswordService _passwordService;
        private readonly JwtService _jwtService;

        public AuthService(AppDbContext context, PasswordService passwordService, JwtService jwtService)
        {
            _context = context;
            _passwordService = passwordService;
            _jwtService = jwtService;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exist = await _context.Users
                .Where(u => u.Email == normalizedEmail)
                .FirstOrDefaultAsync();

            if (exist != null) throw new ArgumentException("El email ya esta registrado.");

            var passwordHash = _passwordService.hashPassword(dto.Password);

            var user = new User
            {
                FirstName = dto.FirstName,
                LastName = dto.LastName,
                Email = normalizedEmail,
                PasswordHash = passwordHash,
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return new RegisterResponse
            {
                Id = user.Id,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email
            };
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

            var user = await _context.Users
                            .Where(u => u.Email == normalizedEmail)
                            .FirstOrDefaultAsync();
            if (user == null) throw new ArgumentException("Usuario no encontrado");

            var isValid = _passwordService.verifyPassword(dto.Password, user.PasswordHash);
            if (!isValid) throw new ArgumentException("Credenciales invalidas");

            var token = _jwtService.GenerateToken(user);
            var refreshToken = _jwtService.GenerateRefreshToken();

            user.RefreshToken = refreshToken;
            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Email = user.Email,
            };
        }

        public async Task<RegisterResponse> RegisterEmployeeAsync(RegisterRequest dto, Guid adminId)
        {
            var adminUserBusiness = await _context.UsersBusiness
                .FirstOrDefaultAsync(ub => ub.UserId == adminId && ub.Role == BusinessRole.Admin);
            if (adminUserBusiness == null) throw new Exception("No tienes permisos para crear servicios");

            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exists = await _context.Users.FirstOrDefaultAsync(u => u.Email == dto.Email);

            if (exists != null) throw new Exception("El email ya esta registrado.");

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var passwordHash = _passwordService.hashPassword(dto.Password);

                var employee = new User
                {
                    FirstName = dto.FirstName,
                    LastName = dto.LastName,
                    Email = dto.Email,
                    PasswordHash = passwordHash,
                };

                _context.Users.Add(employee);
                await _context.SaveChangesAsync();

                var userBusiness = new UserBusiness
                {
                    UserId = employee.Id,
                    BusinessId = adminUserBusiness.BusinessId,
                    Role = BusinessRole.Employee
                };

                _context.UsersBusiness.Add(userBusiness);
                await _context.SaveChangesAsync();

                await transaction.CommitAsync();

                return new RegisterResponse
                {
                    Id = employee.Id,
                    FirstName = employee.FirstName,
                    LastName = employee.LastName,
                    Email = employee.Email,
                };
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();

                throw new Exception(
                    $"Error al registrar empleado. {ex.Message}", ex);
            }
        }
    }
}
