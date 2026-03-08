using Splitza.Api.Models;

namespace Splitza.Api.DTOs;

// --- Requests ---

public record CreateSessionRequest(
    string Name,
    PaymentMode PaymentMode = PaymentMode.HostPays,
    string HostName = "Host"
);

public record UpdateSessionRequest(
    string? Name,
    PaymentMode? PaymentMode,
    decimal? Tip,
    decimal? ServiceFee,
    decimal? Discount,
    AdjustmentAllocation? TipAllocation,
    AdjustmentAllocation? ServiceFeeAllocation,
    AdjustmentAllocation? DiscountAllocation
);

public record FinalizeSessionRequest(
    PaymentMode PaymentMode,
    decimal Tip,
    decimal ServiceFee,
    decimal Discount,
    AdjustmentAllocation TipAllocation,
    AdjustmentAllocation ServiceFeeAllocation,
    AdjustmentAllocation DiscountAllocation,
    // PersonId if AssignedToOne allocation
    Guid? TipAssignedToPersonId,
    Guid? ServiceFeeAssignedToPersonId,
    Guid? DiscountAssignedToPersonId
);

// --- Responses ---

public record SessionSummaryDto(
    Guid Id,
    string Name,
    string Currency,
    PaymentMode PaymentMode,
    SessionStatus Status,
    decimal Subtotal,
    decimal Vat,
    decimal Tip,
    decimal ServiceFee,
    decimal Discount,
    decimal Total,
    DateTime CreatedAt
);

public record SessionDetailDto(
    Guid Id,
    string Name,
    string Currency,
    PaymentMode PaymentMode,
    SessionStatus Status,
    decimal Subtotal,
    decimal Vat,
    decimal Tip,
    decimal ServiceFee,
    decimal Discount,
    decimal Total,
    AdjustmentAllocation TipAllocation,
    AdjustmentAllocation ServiceFeeAllocation,
    AdjustmentAllocation DiscountAllocation,
    DateTime CreatedAt,
    List<PersonDto> People,
    List<ReceiptItemDto> Items
);

public record PersonDto(
    Guid Id,
    string Name,
    bool IsHost
);

public record ReceiptItemDto(
    Guid Id,
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal,
    int SortOrder,
    bool IsManuallyAdded,
    List<AllocationDto> Allocations
);

public record AllocationDto(
    Guid Id,
    Guid PersonId,
    string PersonName,
    AllocationType AllocationType,
    decimal Amount,
    decimal QuantityPortion
);

// --- Person requests ---

public record AddPersonRequest(string Name, bool IsHost = false);

// --- Item requests ---

public record AddItemRequest(
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    bool IsManuallyAdded = true
);

public record UpdateItemRequest(
    string? Name,
    decimal? Quantity,
    decimal? UnitPrice
);

// --- Allocation requests ---

public record SetItemAllocationsRequest(
    List<AllocationInput> Allocations
);

public record AllocationInput(
    Guid PersonId,
    AllocationType AllocationType,
    decimal? CustomAmount,
    decimal? QuantityPortion
);

// --- OCR ---

public record OcrScanRequest(string ImageBase64, string MimeType = "image/jpeg");

public record OcrResultDto(
    string RawText,
    List<ParsedItemDto> ParsedItems,
    decimal? ParsedSubtotal,
    decimal? ParsedVat,
    decimal? ParsedTotal,
    bool ParseSuccess
);

public record ParsedItemDto(
    string Name,
    decimal Quantity,
    decimal UnitPrice,
    decimal LineTotal
);

// --- Calculation ---

public record BillSummaryDto(
    decimal Subtotal,
    decimal Vat,
    decimal Tip,
    decimal ServiceFee,
    decimal Discount,
    decimal Total,
    List<PersonTotalDto> PersonTotals
);

public record PersonTotalDto(
    Guid PersonId,
    string PersonName,
    decimal ItemsSubtotal,
    decimal AdjustmentsShare,
    decimal Total,
    bool IsHost
);
