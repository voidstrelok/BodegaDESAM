# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy project files first to leverage layer caching
COPY BodegaDESAM/BodegaDESAM.csproj BodegaDESAM/
# COPY BodegaDESAM/BodegaDESAM.Client/BodegaDESAM.Client.csproj BodegaDESAM/BodegaDESAM.Client/

RUN dotnet restore BodegaDESAM/BodegaDESAM.csproj

# Copy the rest of the source
COPY . .

RUN dotnet publish BodegaDESAM/BodegaDESAM.csproj \
    -c Release \
    -o /app/publish \
    --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime

# QuestPDF requires fontconfig + at least one font family on Linux
RUN apt-get update \
 && apt-get install -y --no-install-recommends \
        fontconfig \
        fonts-dejavu-core \
 && fc-cache -fv \
 && rm -rf /var/lib/apt/lists/*

WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 8080
ENTRYPOINT ["dotnet", "BodegaDESAM.dll"]
