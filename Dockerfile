# Stage 1: Build (Force .NET 9)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY ./AdoptionSite/AnimalAdoption/*.csproj ./AnimalAdoption/
RUN dotnet restore "./AnimalAdoption/AnimalAdoption.csproj"

# Copy everything else
COPY ./AdoptionSite .

# Build and publish
WORKDIR "/src/AnimalAdoption"
RUN dotnet publish -c Release -o /app

# Stage 2: Runtime (Force .NET 9)
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app .

# Configure environment
ENV ASPNETCORE_ENVIRONMENT=Production
# Remove explicit URL binding to let Railway control the port
# ENV ASPNETCORE_URLS=http://+:3000
#ENV PORT=3000

# Create uploads directory
RUN mkdir -p /app/AnimalUploads

ENTRYPOINT ["dotnet", "AnimalAdoption.dll"]
