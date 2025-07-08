# Use the official .NET 9 runtime as the base image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 8080

# Use the .NET 9 SDK for building
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files and restore dependencies
COPY ["FastTechFoodsAuth.Api/FastTechFoodsAuth.Api.csproj", "FastTechFoodsAuth.Api/"]
COPY ["FastTechFoodsAuth.Security/FastTechFoodsAuth.Security.csproj", "FastTechFoodsAuth.Security/"]
COPY ["FastTechFoodsAuth.Application/FastTechFoodsAuth.Application.csproj", "FastTechFoodsAuth.Application/"]
COPY ["FastTechFoodsAuth.Domain/FastTechFoodsAuth.Domain.csproj", "FastTechFoodsAuth.Domain/"]
COPY ["FastTechFoodsAuth.Infra/FastTechFoodsAuth.Infra.csproj", "FastTechFoodsAuth.Infra/"]

RUN dotnet restore "FastTechFoodsAuth.Api/FastTechFoodsAuth.Api.csproj"

# Copy source code
COPY . .

# Build the application
WORKDIR "/src/FastTechFoodsAuth.Api"
RUN dotnet build "FastTechFoodsAuth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish the application
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "FastTechFoodsAuth.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app

# Create non-root user for security
RUN adduser --disabled-password --gecos '' --shell /bin/bash appuser && chown -R appuser /app
USER appuser

COPY --from=publish /app/publish .

# Health check
HEALTHCHECK --interval=30s --timeout=3s --start-period=5s --retries=3 \
    CMD curl -f http://localhost:8080/health/live || exit 1

ENTRYPOINT ["dotnet", "FastTechFoodsAuth.Api.dll"]