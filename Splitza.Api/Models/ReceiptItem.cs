using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Splitza.Api.Models;

public class ReceiptItem
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid BillSessionId { get; set; }

    [Required, MaxLength(300)]
    public string Name { get; set; } = string.Empty;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Quantity { get; set; } = 1;

    [Column(TypeName = "decimal(18,2)")]
    public decimal UnitPrice { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal LineTotal { get; set; }

    public int SortOrder { get; set; }

    public bool IsManuallyAdded { get; set; }

    public BillSession BillSession { get; set; } = null!;
    public ICollection<ItemAllocation> Allocations { get; set; } = [];
}
