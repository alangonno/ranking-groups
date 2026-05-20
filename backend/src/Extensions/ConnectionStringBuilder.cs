namespace backend.src.Extensions;

public static class ConnectionStringBuilder
{
    public static string BuildFromEnvironment()
    {
        var host = GetEnvOrThrow("DB_HOST");
        var port = GetEnvOrThrow("DB_PORT");
        var database = GetEnvOrThrow("DB_NAME");
        var user = GetEnvOrThrow("DB_USER");
        var password = GetEnvOrThrow("DB_PASSWORD");

        return $"Host={host};Port={port};Database={database};Username={user};Password={password};SSL Mode=Prefer;Trust Server Certificate=true";
    }

    private static string GetEnvOrThrow(string key)
    {
        var value = Environment.GetEnvironmentVariable(key);

        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException($"Environment variable '{key}' is required but not set.");
        }

        return value;
    }
}
