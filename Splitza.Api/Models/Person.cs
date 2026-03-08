using System.ComponentModel.DataAnnotations;

namespace Splitza.Api.Models;

public class Person
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid BillSessionId { get; set; }

    [Required, MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    public bool IsHost { get; set; }

    public BillSession BillSession { get; set; } = null!;
    public ICollection<ItemAllocation> Allocations { get; set; } = [];
}
