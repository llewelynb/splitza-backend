using Splitza.Api.DTOs;
using Splitza.Api.Models;

namespace Splitza.Api.Services;

/// <summary>
/// Pure calculation engine. No DB access — operates on in-memory models.
/// All arithmetic uses decimal to avoid floating-point drift.
/// The sum of all per-person totals is guaranteed to equal the session total.
/// </summary>
public class BillCalculationService
{
    public BillSummaryDto Calculate(
        BillSession session,
        IReadOnlyList<Person> people,
        IReadOnlyList<ReceiptItem> items,
        FinalizeSessionRequest req)
    {
        var personTotals = people.ToDictionary(p => p.Id, p => new PersonRunningTotal(p.Id, p.Name, p.IsHost));

        // 1. Sum each person's item allocations
        foreach (var item in items)
        {
            foreach (var alloc in item.Allocations)
            {
                if (personTotals.TryGetValue(alloc.PersonId, out var pt))
                    pt.ItemsSubtotal += alloc.Amount;
            }
        }

        var subtotal = personTotals.Values.Sum(p => p.ItemsSubtotal);

        // 2. Distribute adjustments
        ApplyAdjustment(req.Tip, req.TipAllocation, req.TipAssignedToPersonId, subtotal, personTotals);
        ApplyAdjustment(req.ServiceFee, req.ServiceFeeAllocation, req.ServiceFeeAssignedToPersonId, subtotal, personTotals);
        // Discount is negative
        ApplyAdjustment(-req.Discount, req.DiscountAllocation, req.DiscountAssignedToPersonId, subtotal, personTotals);

        var total = subtotal + req.Tip + req.ServiceFee - req.Discount;

        // 3. Fix any penny rounding so totals reconcile
        ReconcileRounding(personTotals.Values.ToList(), total);

        return new BillSummaryDto(
            Subtotal: subtotal,
            Vat: session.Vat,
            Tip: req.Tip,
            ServiceFee: req.ServiceFee,
            Discount: req.Discount,
            Total: total,
            PersonTotals: personTotals.Values.Select(pt => new PersonTotalDto(
                pt.PersonId, pt.PersonName,
                pt.ItemsSubtotal, pt.AdjustmentsShare,
                pt.ItemsSubtotal + pt.AdjustmentsShare,
                pt.IsHost
            )).OrderByDescending(p => p.IsHost).ThenBy(p => p.PersonName).ToList()
        );
    }

    private static void ApplyAdjustment(
        decimal amount,
        AdjustmentAllocation mode,
        Guid? assignedToPersonId,
        decimal subtotal,
        Dictionary<Guid, PersonRunningTotal> totals)
    {
        if (amount == 0) return;

        switch (mode)
        {
            case AdjustmentAllocation.Equal:
            {
                var share = decimal.Round(amount / totals.Count, 2);
                var remainder = amount - share * totals.Count;
                bool first = true;
                foreach (var pt in totals.Values)
                {
                    pt.AdjustmentsShare += first ? share + remainder : share;
                    first = false;
                }
                break;
            }
            case AdjustmentAllocation.Proportional:
            {
                if (subtotal == 0) goto case AdjustmentAllocation.Equal;
                decimal allocated = 0;
                PersonRunningTotal? last = null;
                foreach (var pt in totals.Values)
                {
                    var share = decimal.Round(amount * pt.ItemsSubtotal / subtotal, 2);
                    pt.AdjustmentsShare += share;
                    allocated += share;
                    last = pt;
                }
                // Any rounding remainder goes to the last person
                if (last != null) last.AdjustmentsShare += amount - allocated;
                break;
            }
            case AdjustmentAllocation.AssignedToOne:
            {
                if (assignedToPersonId.HasValue && totals.TryGetValue(assignedToPersonId.Value, out var target))
                    target.AdjustmentsShare += amount;
                break;
            }
        }
    }

    private static void ReconcileRounding(List<PersonRunningTotal> totals, decimal expectedTotal)
    {
        var computed = totals.Sum(p => p.ItemsSubtotal + p.AdjustmentsShare);
        var diff = expectedTotal - computed;
        if (diff != 0 && totals.Count > 0)
            totals[0].AdjustmentsShare += diff;
    }

    private sealed class PersonRunningTotal(Guid personId, string personName, bool isHost)
    {
        public Guid PersonId { get; } = personId;
        public string PersonName { get; } = personName;
        public bool IsHost { get; } = isHost;
        public decimal ItemsSubtotal { get; set; }
        public decimal AdjustmentsShare { get; set; }
    }
}
