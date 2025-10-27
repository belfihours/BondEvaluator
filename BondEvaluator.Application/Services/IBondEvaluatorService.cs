using BondEvaluator.Application.Models;

namespace BondEvaluator.Application.Services;

public interface IBondEvaluatorService
{
    Task<Stream> GetBondEvaluation(Stream stream);
}