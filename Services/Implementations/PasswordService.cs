using BCrypt.Net;
using NexusFlow.Services.Interfaces;

namespace NexusFlow.Services.Implementations
{
    public class PasswordService : IPasswordService
    {
        public string hashPassword(string password)
        {
            return BCrypt.Net.BCrypt.HashPassword(password);
        }
        public bool verifyPassword(string password, string hash)
        {
            return BCrypt.Net.BCrypt.Verify(password, hash);
        }
    }
}
