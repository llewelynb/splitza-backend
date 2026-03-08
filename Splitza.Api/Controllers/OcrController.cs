using Microsoft.AspNetCore.Mvc;
using Splitza.Api.DTOs;
using Splitza.Api.Services;

namespace Splitza.Api.Controllers;

[ApiController]
[Route("api/ocr")]
public class OcrController(IOcrService ocrService, ReceiptParserService parser, SessionService sessionService) : ControllerBase
{
    /// <summary>
    /// Submit a base64 image for OCR + parsing. Returns raw text and parsed items for host review.
    /// Does NOT auto-save items — the host reviews first.
    /// </summary>
    [HttpPost("scan")]
    public async Task<ActionResult<OcrResultDto>> Scan(OcrScanRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.ImageBase64))
            return BadRequest("ImageBase64 is required.");

        var rawText = await ocrService.ExtractTextAsync(req.ImageBase64, req.MimeType, ct);
        var result = parser.Parse(rawText);
        return Ok(result);
    }

    /// <summary>
    /// After host confirms/edits OCR results, import items into a session.
    /// </summary>
    [HttpPost("import/{sessionId:guid}")]
    public async Task<ActionResult<SessionDetailDto?>> Import(Guid sessionId, List<ParsedItemDto> items, CancellationToken ct)
    {
        var result = await sessionService.ImportOcrItemsAsync(sessionId, items, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
