using System.Threading.Tasks;
using YourApp.Application.Features.SkinAnalysis.DTOs;

namespace YourApp.Application.Interfaces;

public interface ISkinAnalysisService
{
    Task<AnalysisResponse> AnalyzeAsync(byte[] imageBytes, string skinType);
}
