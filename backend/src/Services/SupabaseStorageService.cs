using backend.src.Common.Exceptions;
using System.Net.Http.Headers;
using System.Text.Json;

namespace backend.src.Services;

public interface ISupabaseStorageService
{
    string GetPublicUrl(string bucket, string path);
    string GetPublicUrlFromPath(string? path);
    Task<string> GenerateUploadSignedUrl(string bucket, string path, int expirySeconds = 300);
    Task DeleteObjectAsync(string bucket, string path);
}

public class SupabaseStorageService : ISupabaseStorageService
{
    private readonly HttpClient _httpClient;
    private readonly string _supabaseUrl;
    private readonly string _serviceKey;

    public SupabaseStorageService()
    {
        _supabaseUrl = GetEnvOrThrow("SUPABASE_URL").TrimEnd('/');
        _serviceKey = GetEnvOrThrow("SUPABASE_SERVICE_KEY");

        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", _serviceKey);
        _httpClient.DefaultRequestHeaders.Add("apikey", _serviceKey);
    }

    public string GetPublicUrl(string bucket, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var cleanPath = path.TrimStart('/');
        return $"{_supabaseUrl}/storage/v1/object/public/{bucket}/{cleanPath}";
    }

    public string GetPublicUrlFromPath(string? path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return string.Empty;

        var parts = path.Split('/', 2);
        if (parts.Length < 2)
            return string.Empty;

        return GetPublicUrl(parts[0], parts[1]);
    }

    public async Task<string> GenerateUploadSignedUrl(string bucket, string path, int expirySeconds = 300)
    {
        var cleanPath = path.TrimStart('/');
        // Endpoint correto: inclui bucket e path na URL, sem body
        var url = $"{_supabaseUrl}/storage/v1/object/upload/sign/{bucket}/{cleanPath}";

        var response = await _httpClient.PostAsync(url, null);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new BusinessRuleException("storage_error", $"Falha ao gerar URL de upload assinada: {errorBody}");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);

        // Supabase Storage retorna: { "url": "/object/upload/sign/bucket/path?token=..." }
        if (doc.RootElement.TryGetProperty("url", out var urlElement))
        {
            var signedPath = urlElement.GetString();
            if (!string.IsNullOrWhiteSpace(signedPath))
            {
                return $"{_supabaseUrl}/storage/v1{signedPath}";
            }
        }

        throw new BusinessRuleException("storage_error", "Resposta inesperada ao gerar URL de upload assinada.");
    }

    public async Task DeleteObjectAsync(string bucket, string path)
    {
        if (string.IsNullOrWhiteSpace(path))
            return;

        var cleanPath = path.TrimStart('/');
        var url = $"{_supabaseUrl}/storage/v1/object/{bucket}/{cleanPath}";

        try
        {
            var response = await _httpClient.DeleteAsync(url);
            response.EnsureSuccessStatusCode();
        }
        catch
        {
            // Silently ignore deletion failures to avoid breaking user operations
        }
    }

    private static string GetEnvOrThrow(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);
        if (string.IsNullOrWhiteSpace(value))
            throw new InvalidOperationException($"Environment variable '{key}' is required but not set.");
        return value;
    }
}
