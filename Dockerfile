# Build Stage
FROM mcr.microsoft.com/dotnet/nightly/sdk:9.0-preview-noble AS build-env
WORKDIR /app

# Create the directories
RUN mkdir -p /media/inputDirectory /media/infuseLibrary

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

# Install FFmpeg and other development tools
RUN apt-get update && apt-get install -y ffmpeg

# Clean up
RUN apt-get clean && rm -rf /var/lib/apt/lists/*

# Copy the built application
COPY --from=build-env /app/out .

# Define 8080 as the port number
ENV ASPNETCORE_URLS http://*:8080
EXPOSE 8080

# Set the entry point for the application
ENTRYPOINT ["dotnet", "Kurmann.Videoschnitt.Application.dll"]