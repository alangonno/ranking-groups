# =========================================
# Stage 1: Build
# =========================================
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /source

# Copy solution and project files first for layer caching
COPY backend.csproj .
COPY tests/tests.csproj ./tests/
RUN dotnet restore backend.csproj

# Copy everything else and build
COPY . .
RUN dotnet publish backend.csproj -c Release -o /app --no-restore

# =========================================
# Stage 2: Runtime
# =========================================
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Create non-root user
RUN addgroup --gid 1000 appgroup && \
    adduser --uid 1000 --ingroup appgroup --disabled-password appuser && \
    chown -R appuser:appgroup /app

# Copy published output
COPY --from=build --chown=appuser:appgroup /app .

# Health check
HEALTHCHECK --interval=30s --timeout=10s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health || exit 1

# Switch to non-root user
USER appuser

# Expose port
EXPOSE 8080

ENTRYPOINT ["dotnet", "backend.dll"]
