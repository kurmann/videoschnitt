# Build Stage
FROM mcr.microsoft.com/dotnet/nightly/sdk:9.0-preview-noble AS build-env
WORKDIR /app

# Copy everything from the src directory
COPY src/ ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish ./Application/Application.csproj -c Release -o out

# Image
FROM mcr.microsoft.com/dotnet/nightly/aspnet:9.0-preview-noble

# Set the working directory
WORKDIR /app

# Copy the built application
COPY --from=build-env /app/out .

# Set the entry point for the application
ENTRYPOINT ["dotnet", "Kurmann.Videoschnitt.Application.dll"]