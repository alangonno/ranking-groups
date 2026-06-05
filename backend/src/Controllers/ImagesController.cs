using backend.src.Common.Exceptions;
using backend.src.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.RegularExpressions;

namespace backend.src.Controllers;

public class GenerateUploadUrlRequest
{
    public string Bucket { get; set; } = string.Empty;
    public string FileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
}

public class GenerateUploadUrlResponse
{
    public string SignedUrl { get; set; } = string.Empty;
    public string PublicUrl { get; set; } = string.Empty;
    public string Path { get; set; } = string.Empty;
}

[ApiController]
[Route("api/images")]
[Authorize]
public class ImagesController : ControllerBase
{
    private readonly ISupabaseStorageService _storageService;
    private readonly ICurrentUserService _currentUserService;

    private static readonly HashSet<string> AllowedBuckets = new() { "avatars", "event-images" };
    private static readonly HashSet<string> AllowedContentTypes = new()
    {
        "image/jpeg",
        "image/png",
        "image/webp"
    };

    public ImagesController(
        ISupabaseStorageService storageService,
        ICurrentUserService currentUserService)
    {
        _storageService = storageService;
        _currentUserService = currentUserService;
    }

    [HttpPost("upload-url")]
    public async Task<IActionResult> GenerateUploadUrl([FromBody] GenerateUploadUrlRequest request, CancellationToken ct)
    {
        ValidateRequest(request);

        var userId = _currentUserService.UserId
            ?? throw new BusinessRuleException("unauthorized", "Usuário não autenticado.");

        var extension = GetExtensionFromContentType(request.ContentType);
        var safeFileName = $"{Guid.NewGuid()}{extension}";

        var path = request.Bucket == "avatars"
            ? $"{userId}/{safeFileName}"
            : $"{safeFileName}";

        var signedUrl = await _storageService.GenerateUploadSignedUrl(request.Bucket, path, expirySeconds: 300);
        var publicUrl = _storageService.GetPublicUrl(request.Bucket, path);

        return Ok(new GenerateUploadUrlResponse
        {
            SignedUrl = signedUrl,
            PublicUrl = publicUrl,
            Path = $"{request.Bucket}/{path}"
        });
    }

    private void ValidateRequest(GenerateUploadUrlRequest request)
    {
        if (!AllowedBuckets.Contains(request.Bucket))
        {
            throw new BusinessRuleException("invalid_bucket", "Bucket não permitido.");
        }

        if (string.IsNullOrWhiteSpace(request.FileName))
        {
            throw new BusinessRuleException("filename_required", "O nome do arquivo é obrigatório.");
        }

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            throw new BusinessRuleException("invalid_content_type", "Tipo de arquivo não permitido. Apenas JPEG, PNG e WebP são aceitos.");
        }
    }

    private static string GetExtensionFromContentType(string contentType)
    {
        return contentType switch
        {
            "image/png" => ".png",
            "image/webp" => ".webp",
            _ => ".jpg"
        };
    }
}
