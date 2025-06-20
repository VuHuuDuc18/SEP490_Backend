# See https://aka.ms/customizecontainer to learn how to customize your debug container and how Visual Studio uses this Dockerfile to build your images for faster debugging.

# Base runtime image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src

# Copy project files
COPY ["SEP490_BackendAPI/SEP490_BackendAPI.csproj", "SEP490_BackendAPI/"]
COPY ["Domain/Domain.csproj", "Domain/"]
COPY ["Entities/Entities.csproj", "Entities/"]
COPY ["Infrastructure/Infrastructure.csproj", "Infrastructure/"]
COPY ["Infrastructure.Identity/Infrastructure.Identity.csproj", "Infrastructure.Identity/"]

# Restore dependencies
RUN dotnet restore "SEP490_BackendAPI/SEP490_BackendAPI.csproj"

# Copy all source code
COPY . .

# Build the application
WORKDIR "/src/SEP490_BackendAPI"
RUN dotnet build "SEP490_BackendAPI.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Publish stage
FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "SEP490_BackendAPI.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

# Final stage
FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# 🔥 Không cần install wait-for-it nữa vì đã có retry logic trong migration
ENTRYPOINT ["dotnet", "SEP490_BackendAPI.dll"]