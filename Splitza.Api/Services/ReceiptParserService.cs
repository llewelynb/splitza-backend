using System.Text.RegularExpressions;
using Splitza.Api.DTOs;

namespace Splitza.Api.Services;

/// <summary>
/// Rule-based receipt parser. Extracts line items and totals from raw OCR text.
/// AI fallback can be added here later without changing callers.
/// </summary>
public partial class ReceiptParserService
{
    // Matches lines like: "Grilled Kingklip   R260.00" or "House Wine x2   R140.00"
    [GeneratedRegex(@"^(.+?)\s+[Rr]?\s*([\d\s]+[\.,]\d{2})\s*$", RegexOptions.Multiline)]
    private static partial Regex ItemLineRegex();

    // Matches totals labelled SUBTOTAL, TOTAL, VAT etc.
    [GeneratedRegex(@"(SUBTOTAL|SUB-TOTAL|SUB TOTAL|TOTAL|VAT|TAX)\s*[Rr]?\s*([\d\s]+[\.,]\d{2})", RegexOptions.IgnoreCase)]
    private static partial Regex TotalLineRegex();

    // Skip these noise labels when parsing item lines
    private static readonly HashSet<string> _skipLabels = new(StringComparer.OrdinalIgnoreCase)
    {
        "subtotal", "sub-total", "sub total", "total", "vat", "tax", "tip",
        "service charge", "service fee", "discount", "change", "cash", "card",
        "amount due", "balance", "thank", "table", "covers", "date", "time"
    };

    public OcrResultDto Parse(string rawText)
    {
        var items = new List<ParsedItemDto>();
        decimal? subtotal = null;
        decimal? vat = null;
        decimal? total = null;

        // Extract totals first so we can exclude those lines from item parsing
        foreach (Match m in TotalLineRegex().Matches(rawText))
        {
            var label = m.Groups[1].Value.ToUpperInvariant();
            var amount = ParseAmount(m.Groups[2].Value);
            if (label is "SUBTOTAL" or "SUB-TOTAL" or "SUB TOTAL") subtotal = amount;
            else if (label is "VAT" or "TAX") vat = amount;
            else if (label is "TOTAL") total = amount;
        }

        // Parse item lines
        int sort = 1;
        foreach (Match m in ItemLineRegex().Matches(rawText))
        {
            var name = m.Groups[1].Value.Trim();

            if (_skipLabels.Any(l => name.Contains(l, StringComparison.OrdinalIgnoreCase)))
                continue;

            if (name.Length < 2) continue;

            var lineTotal = ParseAmount(m.Groups[2].Value);
            if (lineTotal <= 0) continue;

            // Detect quantity prefix like "2x" or "x2"
            var qty = 1m;
            var cleanName = name;
            var qtyMatch = Regex.Match(name, @"^(\d+)[xX]\s+(.+)$|^(.+)\s+[xX](\d+)$");
            if (qtyMatch.Success)
            {
                qty = decimal.Parse(qtyMatch.Groups[1].Success ? qtyMatch.Groups[1].Value : qtyMatch.Groups[4].Value);
                cleanName = (qtyMatch.Groups[2].Success ? qtyMatch.Groups[2].Value : qtyMatch.Groups[3].Value).Trim();
            }

            var unitPrice = qty > 1 ? decimal.Round(lineTotal / qty, 2) : lineTotal;

            items.Add(new ParsedItemDto(cleanName, qty, unitPrice, lineTotal));
            sort++;
        }

        var success = items.Count > 0;

        return new OcrResultDto(rawText, items, subtotal, vat, total, success);
    }

    private static decimal ParseAmount(string raw)
    {
        // Remove spaces used as thousand separators, normalise decimal comma
        var cleaned = raw.Replace(" ", "").Replace(",", ".");
        return decimal.TryParse(cleaned, System.Globalization.NumberStyles.Any,
            System.Globalization.CultureInfo.InvariantCulture, out var result) ? result : 0;
    }
}
