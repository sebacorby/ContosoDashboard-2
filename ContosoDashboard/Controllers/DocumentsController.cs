using System.Security.Claims;
using ContosoDashboard.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ContosoDashboard.Controllers;

[ApiController]
[Authorize]
[Route("api/documents")]
public sealed class DocumentsController : ControllerBase
{
    private readonly IDocumentService _documents;

    public DocumentsController(IDocumentService documents)
    {
        _documents = documents;
    }

    [HttpGet("{documentId:int}/download")]
    public async Task<IActionResult> Download(int documentId, CancellationToken ct)
    {
        var userId = CurrentUserId();
        if (!userId.HasValue) return Forbid();

        var document = await _documents.GetDocumentAsync(documentId, ct);
        if (document is null) return NotFound();
        if (!await _documents.CanAccessAsync(document, userId.Value, ct)) return Forbid();

        try
        {
            var stream = await _documents.OpenContentAsync(document, ct);
            return File(stream, document.FileType, document.OriginalFileName);
        }
        catch (FileNotFoundException)
        {
            return NotFound();
        }
    }

    private int? CurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return int.TryParse(value, out var userId) ? userId : null;
    }
}
