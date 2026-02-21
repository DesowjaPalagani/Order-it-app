# Production multi-stage Dockerfile for OrderIt (ASP.NET Core, .NET 10)
# - Builds the app with the .NET 10 SDK
# - Publishes a framework-dependent app into a lean runtime image
# - Listens on the PORT environment variable (Render-compatible)

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copy project file(s) and restore dependencies
COPY ["OrderIt.csproj", "./"]
RUN dotnet restore "OrderIt.csproj"

# Copy the rest of the source and publish
COPY . .
RUN dotnet publish "OrderIt.csproj" -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app

# Default port if PORT not supplied at runtime
ENV PORT=5000

# Expose a default port for local tools; container will use the runtime PORT env var
EXPOSE 5000

# Copy published output from build stage
COPY --from=build /app/publish .

# Set ASPNETCORE_URLS at container start so it respects the runtime $PORT provided
# by Render or other platforms. Use a shell entry so expansion happens at runtime.
ENTRYPOINT ["sh","-c","export ASPNETCORE_URLS=http://*:${PORT:-5000} && exec dotnet OrderIt.dll"]
