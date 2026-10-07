namespace StoreManagementSystem.Domain.Entities
{
    public class Damage
    {
        public int Id { get; set; }

        public string DamageCode { get; set; } = string.Empty;

        public string DamageName { get; set; } = string.Empty;

        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public bool IsDeleted { get; set; } = false;

        public DateTime CreatedDate { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedDate { get; set; }
    }
}