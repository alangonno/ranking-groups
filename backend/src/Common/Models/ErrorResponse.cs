namespace backend.src.Common.Models;

public class ErrorResponse
{
    public string Type { get; set; } = "business_rule_error";
    public string Message { get; set; } = string.Empty;
    public string? Rule { get; set; }
    public int StatusCode { get; set; } = 400;
}
