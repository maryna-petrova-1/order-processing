# Runtime base image
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files first for restore
COPY ["src/OrderProcessing.Api/OrderProcessing.Api.csproj", "OrderProcessing.Api/"]
COPY ["src/OrderProcessing.Infrastructure/OrderProcessing.Infrastructure.csproj", "OrderProcessing.Infrastructure/"]
COPY ["src/OrderProcessing.Domain/OrderProcessing.Domain.csproj", "OrderProcessing.Domain/"]
COPY ["src/OrderProcessing.Services/OrderProcessing.Services.csproj", "OrderProcessing.Services/"]
COPY ["src/OrderProcessing.Contracts/OrderProcessing.Contracts.csproj", "OrderProcessing.Contracts/"]

# Restore dependencies
WORKDIR /src/OrderProcessing.Api
RUN dotnet restore "OrderProcessing.Api.csproj"

# Copy all source files
COPY src/OrderProcessing.Api/. ./ 
COPY src/OrderProcessing.Infrastructure/. ../OrderProcessing.Infrastructure/
COPY src/OrderProcessing.Domain/. ../OrderProcessing.Domain/
COPY src/OrderProcessing.Services/. ../OrderProcessing.Services/
COPY src/OrderProcessing.Contracts/. ../OrderProcessing.Contracts/

# Publish
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderProcessing.Api.dll"]
