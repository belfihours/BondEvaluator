using BondEvaluator.Application.Services;
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

    [HttpPost(Name = "GetBondEvaluation")]
    public async Task<IActionResult> GetBondEvaluation(IFormFile file)
    {
        // file cannot be null because nullable reference types is enabled in configuration
        if (file.Length == 0)
            return BadRequest("No file uploaded.");
        await using var stream = file.OpenReadStream();
        var res = await _bondEvaluatorService.GetBondEvaluation(stream);
        return File(
            fileStream: stream,
            contentType: "text/csv",
            fileDownloadName: "export.csv"
        );
    }
}