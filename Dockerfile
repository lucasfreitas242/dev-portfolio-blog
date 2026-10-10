# ==============================================================================
# Multi-Stage Production Dockerfile for Dev Portfolio Blog API (.NET 8)
# Optimized for security (non-root 'app' user) and Docker layer caching.
# ==============================================================================

# Stage 1: Build & Publish
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

# Step 1.1: Copy Solution and project files separately for Docker layer caching
COPY PortfolioBlog.slnx ./
COPY src/Blog.Domain/Blog.Domain.csproj src/Blog.Domain/
COPY src/Blog.Application/Blog.Application.csproj src/Blog.Application/
COPY src/Blog.Api/Blog.Api.csproj src/Blog.Api/
COPY tests/Blog.Domain.Tests/Blog.Domain.Tests.csproj tests/Blog.Domain.Tests/

# Step 1.2: Restore package dependencies
RUN dotnet restore src/Blog.Api/Blog.Api.csproj

# Step 1.3: Copy all remaining source code
COPY src/ src/
COPY tests/ tests/

# Step 1.4: Build and publish in Release mode
WORKDIR /src/src/Blog.Api
RUN dotnet publish Blog.Api.csproj -c Release -o /app/publish --no-restore

# Stage 2: Production Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

# Step 2.1: Default ASP.NET Core environment variables for container environments
ENV ASPNETCORE_HTTP_PORTS=8080 \
    ASPNETCORE_ENVIRONMENT=Production \
    DOTNET_SYSTEM_GLOBALIZATION_INVARIANT=false

# Step 2.2: Copy published application artifacts from build stage
COPY --from=build /app/publish .

# Step 2.3: Security best practice - run as non-root built-in 'app' user
USER app

# Step 2.4: Expose default HTTP container port
EXPOSE 8080

# Step 2.5: Entrypoint execution
ENTRYPOINT ["dotnet", "Blog.Api.dll"]
