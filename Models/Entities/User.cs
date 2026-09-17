namespace NexusFlow.Models.Entities
{
    public class User
    {
        public Guid Id { get; set; }
        public string FirstName { get; set; } = string.Empty;
        public string LastName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiry { get; set; }
        public bool IsSuperAdmin { get; set; } = false;
        public ICollection<Appointment> Appointments { get; set; } = [];
        public ICollection<UserBusiness> UserBusinesses { get; set; } = [];
    }
}
