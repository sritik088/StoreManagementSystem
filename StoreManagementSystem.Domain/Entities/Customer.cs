using System.ComponentModel.DataAnnotations;

namespace StoreManagementSystem.Domain.Entities;

public class Customer
{
    public int Id { get; set; }

    [Required]
    [MaxLength(150)]
    public string Name { get; set; } = string.Empty;

    [MaxLength(100)]
    public string? ContactPerson { get; set; }

    [Required]
    [MaxLength(20)]
    public string Mobile { get; set; } = string.Empty;

    [MaxLength(150)]
    public string? Email { get; set; }

    [MaxLength(300)]
    public string? Address { get; set; }

    [MaxLength(100)]
    public string? City { get; set; }

    [MaxLength(100)]
    public string? State { get; set; }

    [MaxLength(15)]
    public string? PostalCode { get; set; }

    [MaxLength(20)]
    public string? GSTNumber { get; set; }

    [MaxLength(20)]
    public string? PANNumber { get; set; }

    public decimal CreditLimit { get; set; }

    public int CreditDays { get; set; }

    public bool IsActive { get; set; } = true;

    public bool IsDeleted { get; set; }

    public DateTime CreatedDate { get; set; }

    public DateTime? UpdatedDate { get; set; }
}
