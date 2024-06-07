# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env
WORKDIR /app

# Copy everything from the src directory
COPY src/ ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish ./Application/Application.csproj -c Release -o out

# Image
FROM mcr.microsoft.com/dotnet/aspnet:7.0-jammy-chiseled

# Set the working directory
WORKDIR /app

# Copy the built application
COPY --from=build-env /app/out .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "Kurmann.Videoschnitt.Application.dll"]