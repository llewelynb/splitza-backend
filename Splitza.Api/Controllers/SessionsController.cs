using Microsoft.AspNetCore.Mvc;
using Splitza.Api.DTOs;
using Splitza.Api.Services;

namespace Splitza.Api.Controllers;

[ApiController]
[Route("api/sessions")]
public class SessionsController(SessionService sessionService) : ControllerBase
{
    [HttpGet]
    public Task<List<SessionSummaryDto>> List(CancellationToken ct) =>
        sessionService.ListAsync(ct);

    [HttpPost]
    public async Task<ActionResult<SessionSummaryDto>> Create(CreateSessionRequest req, CancellationToken ct)
    {
        var result = await sessionService.CreateAsync(req, ct);
        return CreatedAtAction(nameof(Get), new { id = result.Id }, result);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SessionDetailDto>> Get(Guid id, CancellationToken ct)
    {
        var result = await sessionService.GetDetailAsync(id, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}")]
    public async Task<ActionResult<SessionDetailDto>> Update(Guid id, UpdateSessionRequest req, CancellationToken ct)
    {
        var result = await sessionService.UpdateAsync(id, req, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await sessionService.DeleteAsync(id, ct);
        return NoContent();
    }

    [HttpPost("{id:guid}/finalize")]
    public async Task<ActionResult<BillSummaryDto>> Finalize(Guid id, FinalizeSessionRequest req, CancellationToken ct)
    {
        var result = await sessionService.FinalizeAsync(id, req, ct);
        return result is null ? NotFound() : Ok(result);
    }

    // People
    [HttpPost("{id:guid}/people")]
    public async Task<ActionResult<PersonDto>> AddPerson(Guid id, AddPersonRequest req, CancellationToken ct)
    {
        var result = await sessionService.AddPersonAsync(id, req, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}/people/{personId:guid}")]
    public async Task<IActionResult> RemovePerson(Guid id, Guid personId, CancellationToken ct)
    {
        var removed = await sessionService.RemovePersonAsync(id, personId, ct);
        return removed ? NoContent() : NotFound();
    }

    // Items
    [HttpPost("{id:guid}/items")]
    public async Task<ActionResult<ReceiptItemDto>> AddItem(Guid id, AddItemRequest req, CancellationToken ct)
    {
        var result = await sessionService.AddItemAsync(id, req, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPatch("{id:guid}/items/{itemId:guid}")]
    public async Task<ActionResult<ReceiptItemDto>> UpdateItem(Guid id, Guid itemId, UpdateItemRequest req, CancellationToken ct)
    {
        var result = await sessionService.UpdateItemAsync(id, itemId, req, ct);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpDelete("{id:guid}/items/{itemId:guid}")]
    public async Task<IActionResult> DeleteItem(Guid id, Guid itemId, CancellationToken ct)
    {
        var removed = await sessionService.DeleteItemAsync(id, itemId, ct);
        return removed ? NoContent() : NotFound();
    }

    [HttpPut("{id:guid}/items/{itemId:guid}/allocations")]
    public async Task<ActionResult<ReceiptItemDto>> SetAllocations(Guid id, Guid itemId, SetItemAllocationsRequest req, CancellationToken ct)
    {
        var result = await sessionService.SetAllocationsAsync(id, itemId, req, ct);
        return result is null ? NotFound() : Ok(result);
    }
}
