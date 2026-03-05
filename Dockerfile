# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /app

# Copy project files and restore
COPY ["DocGen-Agent.csproj", "./"]
RUN dotnet restore

# Copy source and publish
COPY . .
RUN dotnet publish -c Release -o /app/publish

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/runtime:10.0
WORKDIR /app

# Install infrastructure dependencies (Git)
RUN apt-get update && apt-get install -y \
    git \
    && rm -rf /var/lib/apt/lists/*

# Copy published artifacts
COPY --from=build /app/publish .

# Set entrypoint
ENTRYPOINT ["dotnet", "DocGen-Agent.dll"]
