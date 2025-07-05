FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FastTechFoodsAuth.Api/FastTechFoodsAuth.Api.csproj", "FastTechFoodsAuth.Api/"]
COPY ["FastTechFoodsAuth.Infra/FastTechFoodsAuth.Infra.csproj", "FastTechFoodsAuth.Infra/"]
COPY ["FastTechFoodsAuth.Application/FastTechFoodsAuth.Application.csproj", "FastTechFoodsAuth.Application/"]
COPY ["FastTechFoodsAuth.Domain/FastTechFoodsAuth.Domain.csproj", "FastTechFoodsAuth.Domain/"]
RUN dotnet restore "./FastTechFoodsAuth.Api/FastTechFoodsAuth.Api.csproj"

COPY . .
WORKDIR "/src/FastTechFoodsAuth.Api"
RUN dotnet build "./FastTechFoodsAuth.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "./FastTechFoodsAuth.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FastTechFoodsAuth.Api.dll"]