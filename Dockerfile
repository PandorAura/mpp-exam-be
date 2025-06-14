# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy only the .csproj first to leverage Docker cache
COPY exam-be/WebApplication1/WebApplication1.csproj ./WebApplication1/
RUN dotnet restore ./WebApplication1/WebApplication1.csproj

# Copy the entire source folder
COPY exam-be/WebApplication1 ./WebApplication1

# Publish the app
WORKDIR /src/WebApplication1
RUN dotnet publish -c Release -o /app --no-restore

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app

COPY --from=build /app .

# Set environment for production
ENV ASPNETCORE_ENVIRONMENT=Production
ENV PORT=3000

# Optionally expose the port for documentation
EXPOSE 3000

# Create upload folder (if your app uses this)
RUN mkdir -p /app/AnimalUploads

# Start the app
ENTRYPOINT ["dotnet", "WebApplication1.dll"]
