# Build Stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-env
WORKDIR /app

# Copy everything from the src directory
COPY src/ ./
# Restore as distinct layers
RUN dotnet restore
# Build and publish a release
RUN dotnet publish ./Application/Application.csproj -c Release -o out

# Runtime Image
FROM mcr.microsoft.com/dotnet/aspnet:8.0-jammy-chiseled

# Set the working directory
WORKDIR /app

# Copy the built application
COPY --from=build-env /app/out .

# Specify the non-root user to use (default user ID 1000)
USER 1000:1000

# Expose any necessary ports
EXPOSE 80

# Set the entry point for the application
ENTRYPOINT ["dotnet", "Kurmann.Videoschnitt.Kraftwerk.Application.dll"]
