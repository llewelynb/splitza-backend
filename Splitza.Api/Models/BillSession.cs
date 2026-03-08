using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Splitza.Api.Models;

public class BillSession
{
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    [Required, MaxLength(200)]
    public string Name { get; set; } = string.Empty;

    [Required, MaxLength(3)]
    public string Currency { get; set; } = "ZAR";

    public PaymentMode PaymentMode { get; set; } = PaymentMode.HostPays;

    public SessionStatus Status { get; set; } = SessionStatus.Draft;

    [Column(TypeName = "decimal(18,2)")]
    public decimal Subtotal { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Vat { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Tip { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal ServiceFee { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Discount { get; set; }

    [Column(TypeName = "decimal(18,2)")]
    public decimal Total { get; set; }

    public AdjustmentAllocation TipAllocation { get; set; } = AdjustmentAllocation.Proportional;
    public AdjustmentAllocation ServiceFeeAllocation { get; set; } = AdjustmentAllocation.Equal;
    public AdjustmentAllocation DiscountAllocation { get; set; } = AdjustmentAllocation.Proportional;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<Person> People { get; set; } = [];
    public ICollection<ReceiptItem> Items { get; set; } = [];
}

public enum PaymentMode
{
    HostPays,
    DirectToRestaurant
}

public enum SessionStatus
{
    Draft,
    ItemsReviewed,
    PeopleAdded,
    ItemsAssigned,
    Finalized
}

public enum AdjustmentAllocation
{
    Equal,
    Proportional,
    AssignedToOne
}
