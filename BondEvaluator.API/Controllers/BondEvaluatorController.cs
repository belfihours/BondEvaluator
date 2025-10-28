using System.Net;
using BondEvaluator.Application.Services;
using BondEvaluator.Domain.Models;
using Microsoft.AspNetCore.Mvc;

namespace BondEvaluator.API.Controllers;

[ApiController]
[Route("[controller]")]
public class BondEvaluatorController : ControllerBase
{
    private readonly ILogger<BondEvaluatorController> _logger;
    private readonly IBondEvaluatorService _bondEvaluatorService;

    public BondEvaluatorController(ILogger<BondEvaluatorController> logger, IBondEvaluatorService bondEvaluatorService)
    {
        _logger = logger;
        _bondEvaluatorService = bondEvaluatorService;
    }

    [HttpPost]
    [Route("bondevaluations")]
    [ProducesResponseType(typeof(FileStreamResult), (int)HttpStatusCode.OK)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.BadRequest)]
    [ProducesResponseType(typeof(string), (int)HttpStatusCode.InternalServerError)]
    public async Task<IActionResult> GetBondEvaluation(IFormFile file, CancellationToken ct)
    {
        // file cannot be null because nullable reference types is enabled in configuration
        if (file.Length == 0)
            return BadRequest("No file uploaded.");
        await using var stream = file.OpenReadStream();
        var res = await _bondEvaluatorService.GetBondEvaluation(stream, ct);
        return File(
            fileStream: res,
            contentType: "text/csv",
            fileDownloadName: $"export_{DateTime.Now:yyyy-MM-dd_HH-mm}.csv"
        );
    }
}