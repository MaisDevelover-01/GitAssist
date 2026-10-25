# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build

WORKDIR /src

# Restore dependencies
COPY ["GitAssist.csproj", "./"]
RUN dotnet restore "GitAssist.csproj"

# Copy source files
COPY . .

# Publish application
RUN dotnet publish "GitAssist.csproj" \
    --configuration Release \
    --output /app/publish \
    --no-restore

# Stage 2: Run
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final

WORKDIR /app

COPY --from=build /app/publish/ ./

EXPOSE 10000

ENTRYPOINT ["dotnet", "GitAssist.dll"]