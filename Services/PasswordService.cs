using BCrypt.Net;

namespace NexusFlow.Services
{
    public class PasswordService
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
