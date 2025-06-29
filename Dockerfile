# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY BedAutomation/BedAutomation.csproj BedAutomation/
RUN dotnet restore "BedAutomation/BedAutomation.csproj"

# Copy everything else and build
COPY . .
WORKDIR "/src/BedAutomation"
RUN dotnet build "BedAutomation.csproj" -c Release -o /app/build

# Publish stage
FROM build AS publish
RUN dotnet publish "BedAutomation.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app

COPY --from=publish /app/publish .

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:8080

EXPOSE 8080

ENTRYPOINT ["dotnet", "BedAutomation.dll"] 