using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Splitza.Api.Models;

public class ItemAllocation
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required]
    public Guid ReceiptItemId { get; set; }

    [Required]
    public Guid PersonId { get; set; }

    public AllocationType AllocationType { get; set; }

    // The resolved rand amount this person owes for this item
    [Column(TypeName = "decimal(18,2)")]
    public decimal Amount { get; set; }

    // For QuantityShare: how many units this person takes
    [Column(TypeName = "decimal(18,2)")]
    public decimal QuantityPortion { get; set; }

    public ReceiptItem ReceiptItem { get; set; } = null!;
    public Person Person { get; set; } = null!;
}

public enum AllocationType
{
    Full,
    EqualShare,
    QuantityShare,
    CustomAmount
}
