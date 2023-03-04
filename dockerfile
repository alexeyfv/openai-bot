# Start with a base image that includes .NET Core
FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build-env

# Set the working directory to /app
WORKDIR /app

# Copy the .csproj file and restore dependencies
COPY *.csproj ./
RUN dotnet restore

# Copy the rest of the application code
COPY . ./

# Build the application and publish to a directory called "out"
RUN dotnet publish -c Release -o out

# Start with a new image that just includes .NET Core runtime
FROM mcr.microsoft.com/dotnet/runtime:7.0

# Set the working directory to /app
WORKDIR /app

# Copy the published application files from the build image to this image
COPY --from=build-env /app/out ./

# Start the worker service
ENTRYPOINT ["dotnet", "openai-bot.dll"]
