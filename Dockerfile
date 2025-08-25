# Imagen base de runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0-alpine AS base
WORKDIR /app
EXPOSE 8080

# Imagen de build
FROM mcr.microsoft.com/dotnet/sdk:9.0-alpine AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copiar csproj y restaurar dependencias
COPY LatinoNetOnline.MCP.Server.csproj .
RUN dotnet restore "LatinoNetOnline.MCP.Server.csproj"

# Copiar el resto del código y compilar
COPY . .
RUN dotnet publish "LatinoNetOnline.MCP.Server.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Imagen final
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .

# Variables para Azure Container Apps
ENV PORT=8080
ENV ASPNETCORE_URLS=http://+:8080

ENTRYPOINT ["dotnet", "LatinoNetOnline.MCP.Server.dll"]
