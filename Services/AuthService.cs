using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexusFlow.Data;
using NexusFlow.Models.DTOs;
using NexusFlow.Models.Entities;

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

        public async Task RegisterAsync(RegisterRequest dto)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            var exist = await _context.Users
                .Where(u => u.Email == normalizedEmail)
                .FirstOrDefaultAsync();

            if (exist != null) throw new ArgumentException("El email ya esta registrado");

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
        }

        public async Task<LoginResponse> Login(LoginRequest dto)
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

            await _context.SaveChangesAsync();

            return new LoginResponse
            {
                Token = token,
                RefreshToken = refreshToken,
                UserId = user.Id,
                Email = user.Email,
            };
        }

    }
}
