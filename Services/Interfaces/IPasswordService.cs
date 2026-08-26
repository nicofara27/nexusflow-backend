namespace NexusFlow.Services.Interfaces
{
    public interface IPasswordService
    {
        string hashPassword(string password);
        bool verifyPassword(string password, string hash);
    }
}
