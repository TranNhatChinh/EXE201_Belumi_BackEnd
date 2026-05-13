using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Processing;
using YourApp.Application.Interfaces;
using YourApp.Application.Features.SkinAnalysis.DTOs;

namespace YourApp.Infrastructure.Services;

public class SkinAnalysisService : ISkinAnalysisService
{
    private readonly HttpClient _httpClient;
    private readonly IMemoryCache _cache;
    private readonly ILogger<SkinAnalysisService> _logger;
    private readonly string _apiKey;

    private const string GeminiUrl =
        "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

    public SkinAnalysisService(
        HttpClient httpClient,
        IMemoryCache cache,
        ILogger<SkinAnalysisService> logger,
        IConfiguration config)
    {
        _httpClient = httpClient;
        _cache = cache;
        _logger = logger;
        _apiKey = config["Gemini:ApiKey"]
            ?? throw new InvalidOperationException("Gemini:ApiKey chưa được config trong appsettings.json");
    }

    public async Task<AnalysisResponse> AnalyzeAsync(byte[] imageBytes, string skinType)
    {
        var validationError = ValidateImage(imageBytes);
        if (validationError != null)
        {
            _logger.LogWarning("Image validation failed: {Error}", validationError);
            return new AnalysisResponse { Status = "retake_required", Message = validationError };
        }

        byte[] processedImage;
        try
        {
            processedImage = PreprocessImage(imageBytes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Image preprocessing failed");
            return new AnalysisResponse
            {
                Status = "error",
                Message = $"Ảnh không hợp lệ hoặc bị lỗi: {ex.Message}"
            };
        }

        var imageHash = ComputeHash(processedImage);
        var cacheKey = $"skin:{skinType}:{imageHash}";

        if (_cache.TryGetValue(cacheKey, out SkinAnalysisResult? cached) && cached != null)
        {
            _logger.LogInformation("Cache hit for key {CacheKey}", cacheKey);
            return new AnalysisResponse
            {
                Status = "success",
                Result = cached,
                FromCache = true
            };
        }

        var base64Image = Convert.ToBase64String(processedImage);

        SkinAnalysisResult? result;
        try
        {
            result = await CallGeminiAsync(base64Image, skinType);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini API call failed");
            return new AnalysisResponse { Status = "error", Message = $"Lỗi AI: {ex.Message}" };
        }

        if (result == null)
        {
            _logger.LogWarning("Gemini returned null result for skinType={SkinType}", skinType);
            return new AnalysisResponse
            {
                Status = "error",
                Message = "AI không trả về kết quả hợp lệ"
            };
        }

        _logger.LogInformation("Gemini confidence={Confidence}, score={Score}, acne={Acne}",
            result.Confidence, result.OverallScore, result.AcneLevel);

        if (result.Confidence < 0.65)
        {
            return new AnalysisResponse
            {
                Status = "retake_required",
                Message = "Ảnh chưa đủ rõ. Vui lòng chụp ở nơi có ánh sáng tốt hơn."
            };
        }

        result.SkinCondition = GenerateSkinCondition(result);
        result.Description    = GenerateDescription(result, skinType);

        _cache.Set(cacheKey, result, TimeSpan.FromHours(24));

        return new AnalysisResponse { Status = "success", Result = result, FromCache = false };
    }

    private string GenerateSkinCondition(SkinAnalysisResult result)
    {
        var concernCount = 0;
        if (result.AcneLevel != "none") concernCount++;
        if (result.DarkSpots)           concernCount++;
        if (result.EnlargedPores)       concernCount++;
        if (result.Redness)             concernCount++;
        if (result.UnevenTone)          concernCount++;

        return result.AcneLevel switch
        {
            "severe"   => "critical",
            "moderate" => "needs_care",
            "mild"     => "needs_attention",
            "none"     => concernCount >= 2 ? "needs_attention" : "good",
            _          => "good"
        };
    }

    private string GenerateDescription(SkinAnalysisResult result, string skinType)
    {
        var parts = new List<string>();

        var tier = result.AcneLevel switch
        {
            "none"     => "Da bạn hiện đang trong tình trạng khá tốt",
            "mild"     => "Da bạn đang có một vài vấn đề nhỏ cần chú ý",
            "moderate" => "Da bạn đang cần được chăm sóc tích cực hơn",
            "severe"   => "Da bạn đang có dấu hiệu cần được chú trọng chăm sóc",
            _          => "Da bạn đang trong trạng thái bình thường"
        };
        parts.Add(tier);

        var concerns = new List<string>();
        if (result.AcneLevel != "none") concerns.Add("mụn");
        if (result.DarkSpots)           concerns.Add("thâm");
        if (result.EnlargedPores)       concerns.Add("lỗ chân lông to");
        if (result.Redness)             concerns.Add("vùng đỏ");
        if (result.UnevenTone)          concerns.Add("da không đều màu");

        if (concerns.Any())
            parts.Add($"Hiện tại nhìn thấy: {string.Join(", ", concerns)}");

        var tip = skinType switch
        {
            "oily"        => "Nên tập trung kiểm soát dầu và làm sạch đúng cách",
            "dry"         => "Nên ưu tiên dưỡng ẩm và tránh sản phẩm gây khô da",
            "combination" => "Nên chăm sóc khác nhau cho vùng T và vùng má",
            "sensitive"   => "Nên ưu tiên sản phẩm dịu nhẹ, tránh thành phần kích ứng",
            _             => "Nên duy trì routine chăm sóc da đều đặn"
        };
        parts.Add(tip);

        return string.Join(". ", parts) + ".";
    }

    private string? ValidateImage(byte[] imageBytes)
    {
        if (imageBytes.Length > 5 * 1024 * 1024)
            return "Ảnh quá lớn. Vui lòng chọn ảnh nhỏ hơn 5MB.";

        if (!IsJpegOrPng(imageBytes))
            return "Định dạng ảnh không hợp lệ. Chỉ hỗ trợ JPEG và PNG.";

        return null;
    }

    private static bool IsJpegOrPng(byte[] bytes)
    {
        if (bytes.Length < 4) return false;

        bool isJpeg = bytes[0] == 0xFF && bytes[1] == 0xD8 && bytes[2] == 0xFF;
        bool isPng  = bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47;

        return isJpeg || isPng;
    }

    private static byte[] PreprocessImage(byte[] imageBytes)
    {
        using var input = new MemoryStream(imageBytes);
        using var image = Image.Load(input);

        image.Mutate(ctx => ctx
            .Resize(new ResizeOptions
            {
                Size     = new Size(512, 512),
                Mode     = ResizeMode.Pad,
                Position = AnchorPositionMode.Center
            })
            .Brightness(1.05f)
            .Contrast(1.05f)
        );

        using var output = new MemoryStream();
        image.Save(output, new JpegEncoder { Quality = 85 });
        return output.ToArray();
    }

    private static string ComputeHash(byte[] imageBytes)
    {
        var hash = SHA256.HashData(imageBytes);
        return Convert.ToHexString(hash).ToLower();
    }

    private async Task<SkinAnalysisResult?> CallGeminiAsync(string base64Image, string skinType)
    {
        var schema = new JsonObject
        {
            ["type"] = "object",
            ["properties"] = new JsonObject
            {
                ["acne_level"] = new JsonObject
                {
                    ["type"] = "string",
                    ["enum"] = new JsonArray("none", "mild", "moderate", "severe")
                },
                ["dark_spots"]      = new JsonObject { ["type"] = "boolean" },
                ["enlarged_pores"]  = new JsonObject { ["type"] = "boolean" },
                ["redness"]         = new JsonObject { ["type"] = "boolean" },
                ["uneven_tone"]     = new JsonObject { ["type"] = "boolean" },
                ["top_concerns"] = new JsonObject
                {
                    ["type"] = "array",
                    ["items"] = new JsonObject
                    {
                        ["type"] = "string",
                        ["enum"] = new JsonArray("acne", "dark_spots", "pores",
                                                  "dryness", "oiliness", "dullness", "redness")
                    },
                    ["maxItems"] = 3
                },
                ["overall_score"] = new JsonObject
                {
                    ["type"]    = "integer",
                    ["minimum"] = 0,
                    ["maximum"] = 100
                },
                ["confidence"] = new JsonObject
                {
                    ["type"]    = "number",
                    ["minimum"] = 0.0,
                    ["maximum"] = 1.0
                }
            },
            ["required"] = new JsonArray(
                "acne_level", "dark_spots", "enlarged_pores",
                "redness", "uneven_tone", "top_concerns",
                "overall_score", "confidence"
            )
        };

        var requestBody = new JsonObject
        {
            ["contents"] = new JsonArray
            {
                new JsonObject
                {
                    ["parts"] = new JsonArray
                    {
                        new JsonObject { ["text"] = BuildPrompt(skinType) },
                        new JsonObject
                        {
                            ["inline_data"] = new JsonObject
                            {
                                ["mime_type"] = "image/jpeg",
                                ["data"]      = base64Image
                            }
                        }
                    }
                }
            },
            ["generationConfig"] = new JsonObject
            {
                ["response_mime_type"] = "application/json",
                ["response_schema"]    = schema,
                ["temperature"]        = 0.1,
                ["maxOutputTokens"]    = 1024
            }
        };

        var requestUri = $"{GeminiUrl}?key={_apiKey}";
        var content = new StringContent(requestBody.ToJsonString(), Encoding.UTF8, "application/json");

        var response = await _httpClient.PostAsync(requestUri, content);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError("Gemini API error {StatusCode}: {Body}", response.StatusCode, errorBody);
            throw new HttpRequestException($"Gemini API lỗi {response.StatusCode}: {errorBody}");
        }

        var responseBody = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(responseBody);

        var resultText = doc.RootElement
            .GetProperty("candidates")[0]
            .GetProperty("content")
            .GetProperty("parts")[0]
            .GetProperty("text")
            .GetString();

        if (string.IsNullOrWhiteSpace(resultText))
        {
            _logger.LogWarning("Gemini returned empty text in response");
            return null;
        }

        return JsonSerializer.Deserialize<SkinAnalysisResult>(resultText);
    }

    private static string BuildPrompt(string skinType) => $"""
        You are a professional skincare analysis AI.

        The user has already been identified as having {skinType} skin.
        Do NOT re-classify skin type — it is already known.

        Your task: Analyze the facial image and identify VISIBLE skin concerns only.

        Look for:
        - Acne or pimples (none/mild/moderate/severe)
        - Dark spots or hyperpigmentation (yes/no)
        - Enlarged or visible pores (yes/no)
        - Redness or inflammation (yes/no)
        - Uneven skin tone or dullness (yes/no)

        overall_score: Rate overall skin health from 0 (severe issues) to 100 (perfect skin).
        confidence: How confident you are in this analysis based on image quality (0.0 to 1.0).

        Respond ONLY with valid JSON matching the schema. No explanations, no markdown.
        """;
}
