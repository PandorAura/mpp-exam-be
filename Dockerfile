# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the .csproj file first to leverage Docker cache
COPY exam-be/WebApplication1/WebApplication1.csproj WebApplication1/

# Restore packages
RUN dotnet restore WebApplication1/WebApplication1.csproj

# Copy the entire source folder
COPY exam-be/WebApplication1/ WebApplication1/

# Publish the app (note: publish the .csproj, not solution)
RUN dotnet publish WebApplication1/WebApplication1.csproj -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy the published app from build stage
COPY --from=build /app ./

# Set environment variables
ENV ASPNETCORE_ENVIRONMENT=Production \
    PORT=3000

# Optionally expose the port
EXPOSE 3000

# Create upload folder (if used)
RUN mkdir -p /app/AnimalUploads

# Start the app
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
