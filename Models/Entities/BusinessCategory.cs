namespace NexusFlow.Models.Entities
{
    public class BusinessCategory
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty; 
        public string Slug { get; set; } = string.Empty;
        public ICollection<Business> Businesses { get; set; } = [];
    }
}
