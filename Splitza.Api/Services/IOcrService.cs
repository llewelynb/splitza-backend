using Splitza.Api.DTOs;

namespace Splitza.Api.Services;

/// <summary>
/// Pluggable OCR abstraction. Swap the implementation for real OCR without touching the rest of the app.
/// </summary>
public interface IOcrService
{
    Task<string> ExtractTextAsync(string imageBase64, string mimeType, CancellationToken ct = default);
}

/// <summary>
/// Mock OCR implementation. Returns realistic demo text so the app can be developed end-to-end
/// without a real OCR provider. Replace with a real provider (e.g. Google Vision, Azure Computer Vision)
/// by registering a different implementation in Program.cs.
/// </summary>
public class MockOcrService : IOcrService
{
    public Task<string> ExtractTextAsync(string imageBase64, string mimeType, CancellationToken ct = default)
    {
        // Simulates typical South African restaurant receipt OCR output
        var mockText = """
            HARBOUR HOUSE WATERFRONT
            V&A Waterfront, Cape Town
            Tel: 021 000 0000
            VAT No: 4000000000

            TABLE: 12       COVERS: 2
            DATE: 08/03/2026   TIME: 19:45

            Grilled Kingklip              R260.00
            Calamari Starter              R120.00
            House Wine                     R70.00
            House Wine                     R70.00

            SUBTOTAL                      R520.00
            15% TIP (optional)             R78.00
            --------------------------------
            TOTAL                         R598.00

            Thank you for dining with us!
            """;

        return Task.FromResult(mockText);
    }
}
