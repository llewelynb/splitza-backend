using Microsoft.EntityFrameworkCore;
using Splitza.Api.Data;
using Splitza.Api.DTOs;
using Splitza.Api.Models;

namespace Splitza.Api.Services;

public class SessionService(AppDbContext db, BillCalculationService calculator)
{
    public async Task<SessionSummaryDto> CreateAsync(CreateSessionRequest req, CancellationToken ct)
    {
        var session = new BillSession
        {
            Name = req.Name,
            PaymentMode = req.PaymentMode
        };
        db.BillSessions.Add(session);

        // Auto-create host person
        db.People.Add(new Person
        {
            BillSessionId = session.Id,
            Name = req.HostName,
            IsHost = true
        });

        await db.SaveChangesAsync(ct);
        return ToSummary(session);
    }

    public async Task<SessionDetailDto?> GetDetailAsync(Guid id, CancellationToken ct)
    {
        var session = await db.BillSessions
            .Include(s => s.People)
            .Include(s => s.Items).ThenInclude(i => i.Allocations).ThenInclude(a => a.Person)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

        return session is null ? null : ToDetail(session);
    }

    public async Task<List<SessionSummaryDto>> ListAsync(CancellationToken ct)
    {
        return await db.BillSessions
            .OrderByDescending(s => s.CreatedAt)
            .Select(s => ToSummary(s))
            .ToListAsync(ct);
    }

    public async Task<SessionDetailDto?> UpdateAsync(Guid id, UpdateSessionRequest req, CancellationToken ct)
    {
        var session = await LoadFullAsync(id, ct);
        if (session is null) return null;

        if (req.Name is not null) session.Name = req.Name;
        if (req.PaymentMode.HasValue) session.PaymentMode = req.PaymentMode.Value;
        if (req.Tip.HasValue) session.Tip = req.Tip.Value;
        if (req.ServiceFee.HasValue) session.ServiceFee = req.ServiceFee.Value;
        if (req.Discount.HasValue) session.Discount = req.Discount.Value;
        if (req.TipAllocation.HasValue) session.TipAllocation = req.TipAllocation.Value;
        if (req.ServiceFeeAllocation.HasValue) session.ServiceFeeAllocation = req.ServiceFeeAllocation.Value;
        if (req.DiscountAllocation.HasValue) session.DiscountAllocation = req.DiscountAllocation.Value;

        session.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToDetail(session);
    }

    public async Task<BillSummaryDto?> FinalizeAsync(Guid id, FinalizeSessionRequest req, CancellationToken ct)
    {
        var session = await LoadFullAsync(id, ct);
        if (session is null) return null;

        session.PaymentMode = req.PaymentMode;
        session.Tip = req.Tip;
        session.ServiceFee = req.ServiceFee;
        session.Discount = req.Discount;
        session.TipAllocation = req.TipAllocation;
        session.ServiceFeeAllocation = req.ServiceFeeAllocation;
        session.DiscountAllocation = req.DiscountAllocation;
        session.Status = SessionStatus.Finalized;
        session.UpdatedAt = DateTime.UtcNow;

        var summary = calculator.Calculate(session, session.People.ToList(), session.Items.ToList(), req);
        session.Total = summary.Total;
        session.Subtotal = summary.Subtotal;

        await db.SaveChangesAsync(ct);
        return summary;
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct)
    {
        var session = await db.BillSessions.FindAsync([id], ct);
        if (session is not null)
        {
            db.BillSessions.Remove(session);
            await db.SaveChangesAsync(ct);
        }
    }

    // People management
    public async Task<PersonDto?> AddPersonAsync(Guid sessionId, AddPersonRequest req, CancellationToken ct)
    {
        var exists = await db.BillSessions.AnyAsync(s => s.Id == sessionId, ct);
        if (!exists) return null;

        var person = new Person { BillSessionId = sessionId, Name = req.Name, IsHost = req.IsHost };
        db.People.Add(person);
        await db.SaveChangesAsync(ct);
        return new PersonDto(person.Id, person.Name, person.IsHost);
    }

    public async Task<bool> RemovePersonAsync(Guid sessionId, Guid personId, CancellationToken ct)
    {
        var person = await db.People.FirstOrDefaultAsync(p => p.Id == personId && p.BillSessionId == sessionId, ct);
        if (person is null) return false;
        db.People.Remove(person);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // Item management
    public async Task<ReceiptItemDto?> AddItemAsync(Guid sessionId, AddItemRequest req, CancellationToken ct)
    {
        var exists = await db.BillSessions.AnyAsync(s => s.Id == sessionId, ct);
        if (!exists) return null;

        var maxOrder = await db.ReceiptItems.Where(i => i.BillSessionId == sessionId)
            .MaxAsync(i => (int?)i.SortOrder, ct) ?? 0;

        var item = new ReceiptItem
        {
            BillSessionId = sessionId,
            Name = req.Name,
            Quantity = req.Quantity,
            UnitPrice = req.UnitPrice,
            LineTotal = decimal.Round(req.Quantity * req.UnitPrice, 2),
            SortOrder = maxOrder + 1,
            IsManuallyAdded = req.IsManuallyAdded
        };
        db.ReceiptItems.Add(item);
        await UpdateSessionSubtotal(sessionId, ct);
        await db.SaveChangesAsync(ct);
        return ToItemDto(item);
    }

    public async Task<ReceiptItemDto?> UpdateItemAsync(Guid sessionId, Guid itemId, UpdateItemRequest req, CancellationToken ct)
    {
        var item = await db.ReceiptItems
            .Include(i => i.Allocations).ThenInclude(a => a.Person)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.BillSessionId == sessionId, ct);
        if (item is null) return null;

        if (req.Name is not null) item.Name = req.Name;
        if (req.Quantity.HasValue) item.Quantity = req.Quantity.Value;
        if (req.UnitPrice.HasValue) item.UnitPrice = req.UnitPrice.Value;
        item.LineTotal = decimal.Round(item.Quantity * item.UnitPrice, 2);

        await UpdateSessionSubtotal(sessionId, ct);
        await db.SaveChangesAsync(ct);
        return ToItemDto(item);
    }

    public async Task<bool> DeleteItemAsync(Guid sessionId, Guid itemId, CancellationToken ct)
    {
        var item = await db.ReceiptItems.FirstOrDefaultAsync(i => i.Id == itemId && i.BillSessionId == sessionId, ct);
        if (item is null) return false;
        db.ReceiptItems.Remove(item);
        await UpdateSessionSubtotal(sessionId, ct);
        await db.SaveChangesAsync(ct);
        return true;
    }

    // Allocation
    public async Task<ReceiptItemDto?> SetAllocationsAsync(Guid sessionId, Guid itemId, SetItemAllocationsRequest req, CancellationToken ct)
    {
        var item = await db.ReceiptItems
            .Include(i => i.Allocations)
            .FirstOrDefaultAsync(i => i.Id == itemId && i.BillSessionId == sessionId, ct);
        if (item is null) return null;

        var people = await db.People.Where(p => p.BillSessionId == sessionId).ToListAsync(ct);

        // Remove existing allocations
        db.ItemAllocations.RemoveRange(item.Allocations);

        // Resolve amounts
        var newAllocations = ResolveAllocations(item, req.Allocations, people);
        db.ItemAllocations.AddRange(newAllocations);
        await db.SaveChangesAsync(ct);

        // Reload with person nav
        item = await db.ReceiptItems
            .Include(i => i.Allocations).ThenInclude(a => a.Person)
            .FirstAsync(i => i.Id == itemId, ct);

        return ToItemDto(item);
    }

    // OCR import: bulk-import parsed items into the session
    public async Task<SessionDetailDto?> ImportOcrItemsAsync(Guid sessionId, List<ParsedItemDto> parsedItems, CancellationToken ct)
    {
        var session = await db.BillSessions.FindAsync([sessionId], ct);
        if (session is null) return null;

        // Remove existing non-manual items
        var existing = await db.ReceiptItems
            .Where(i => i.BillSessionId == sessionId && !i.IsManuallyAdded)
            .ToListAsync(ct);
        db.ReceiptItems.RemoveRange(existing);

        int sort = 1;
        foreach (var parsed in parsedItems)
        {
            db.ReceiptItems.Add(new ReceiptItem
            {
                BillSessionId = sessionId,
                Name = parsed.Name,
                Quantity = parsed.Quantity,
                UnitPrice = parsed.UnitPrice,
                LineTotal = parsed.LineTotal,
                SortOrder = sort++,
                IsManuallyAdded = false
            });
        }

        await UpdateSessionSubtotal(sessionId, ct);
        session.Status = SessionStatus.ItemsReviewed;
        session.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);

        return await GetDetailAsync(sessionId, ct);
    }

    // Helpers
    private async Task UpdateSessionSubtotal(Guid sessionId, CancellationToken ct)
    {
        var session = await db.BillSessions.FindAsync([sessionId], ct);
        if (session is null) return;
        session.Subtotal = await db.ReceiptItems
            .Where(i => i.BillSessionId == sessionId)
            .SumAsync(i => i.LineTotal, ct);
        session.Total = session.Subtotal + session.Tip + session.ServiceFee - session.Discount;
        session.UpdatedAt = DateTime.UtcNow;
    }

    private static List<ItemAllocation> ResolveAllocations(
        ReceiptItem item,
        List<AllocationInput> inputs,
        List<Person> people)
    {
        var result = new List<ItemAllocation>();
        if (inputs.Count == 0) return result;

        switch (inputs[0].AllocationType)
        {
            case AllocationType.Full:
            {
                var input = inputs[0];
                result.Add(new ItemAllocation
                {
                    ReceiptItemId = item.Id,
                    PersonId = input.PersonId,
                    AllocationType = AllocationType.Full,
                    Amount = item.LineTotal
                });
                break;
            }
            case AllocationType.EqualShare:
            {
                var share = decimal.Round(item.LineTotal / inputs.Count, 2);
                var remainder = item.LineTotal - share * inputs.Count;
                for (int i = 0; i < inputs.Count; i++)
                {
                    result.Add(new ItemAllocation
                    {
                        ReceiptItemId = item.Id,
                        PersonId = inputs[i].PersonId,
                        AllocationType = AllocationType.EqualShare,
                        Amount = i == 0 ? share + remainder : share
                    });
                }
                break;
            }
            case AllocationType.QuantityShare:
            {
                var totalQty = inputs.Sum(i => i.QuantityPortion ?? 1);
                foreach (var input in inputs)
                {
                    var qty = input.QuantityPortion ?? 1;
                    result.Add(new ItemAllocation
                    {
                        ReceiptItemId = item.Id,
                        PersonId = input.PersonId,
                        AllocationType = AllocationType.QuantityShare,
                        Amount = decimal.Round(item.LineTotal * qty / totalQty, 2),
                        QuantityPortion = qty
                    });
                }
                break;
            }
            case AllocationType.CustomAmount:
            {
                foreach (var input in inputs)
                {
                    result.Add(new ItemAllocation
                    {
                        ReceiptItemId = item.Id,
                        PersonId = input.PersonId,
                        AllocationType = AllocationType.CustomAmount,
                        Amount = input.CustomAmount ?? 0
                    });
                }
                break;
            }
        }

        return result;
    }

    private async Task<BillSession?> LoadFullAsync(Guid id, CancellationToken ct) =>
        await db.BillSessions
            .Include(s => s.People)
            .Include(s => s.Items).ThenInclude(i => i.Allocations)
            .FirstOrDefaultAsync(s => s.Id == id, ct);

    private static SessionSummaryDto ToSummary(BillSession s) =>
        new(s.Id, s.Name, s.Currency, s.PaymentMode, s.Status,
            s.Subtotal, s.Vat, s.Tip, s.ServiceFee, s.Discount, s.Total, s.CreatedAt);

    private static SessionDetailDto ToDetail(BillSession s) =>
        new(s.Id, s.Name, s.Currency, s.PaymentMode, s.Status,
            s.Subtotal, s.Vat, s.Tip, s.ServiceFee, s.Discount, s.Total,
            s.TipAllocation, s.ServiceFeeAllocation, s.DiscountAllocation,
            s.CreatedAt,
            s.People.Select(p => new PersonDto(p.Id, p.Name, p.IsHost)).ToList(),
            s.Items.OrderBy(i => i.SortOrder).Select(ToItemDto).ToList());

    private static ReceiptItemDto ToItemDto(ReceiptItem i) =>
        new(i.Id, i.Name, i.Quantity, i.UnitPrice, i.LineTotal, i.SortOrder, i.IsManuallyAdded,
            i.Allocations.Select(a => new AllocationDto(
                a.Id, a.PersonId, a.Person?.Name ?? "", a.AllocationType, a.Amount, a.QuantityPortion
            )).ToList());
}
