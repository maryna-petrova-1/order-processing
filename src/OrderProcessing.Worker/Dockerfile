# Runtime base image
FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app

# Build stage
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Copy csproj files
COPY ["src/OrderProcessing.Worker/OrderProcessing.Worker.csproj", "OrderProcessing.Worker/"]
COPY ["src/OrderProcessing.Infrastructure/OrderProcessing.Infrastructure.csproj", "OrderProcessing.Infrastructure/"]
COPY ["src/OrderProcessing.Domain/OrderProcessing.Domain.csproj", "OrderProcessing.Domain/"]
COPY ["src/OrderProcessing.Services/OrderProcessing.Services.csproj", "OrderProcessing.Services/"]
COPY ["src/OrderProcessing.Contracts/OrderProcessing.Contracts.csproj", "OrderProcessing.Contracts/"]

# Restore dependencies for the Worker project
WORKDIR /src/OrderProcessing.Worker
RUN dotnet restore "OrderProcessing.Worker.csproj"

# Copy all source code
COPY src/OrderProcessing.Worker/. ./ 
COPY src/OrderProcessing.Infrastructure/. ../OrderProcessing.Infrastructure/
COPY src/OrderProcessing.Domain/. ../OrderProcessing.Domain/
COPY src/OrderProcessing.Services/. ../OrderProcessing.Services/
COPY src/OrderProcessing.Contracts/. ../OrderProcessing.Contracts/

# Publish Worker project
RUN dotnet publish -c Release -o /app/publish

# Final runtime image
FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "OrderProcessing.Worker.dll"]
