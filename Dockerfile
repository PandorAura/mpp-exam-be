# Stage 1: Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

ENV DOTNET_NOLOGO=true \
    DOTNET_SKIP_FIRST_TIME_EXPERIENCE=true \
    DOTNET_ROLL_FORWARD=LatestMajor \
    NUGET_FALLBACK_PACKAGES=""

# Copy only the .csproj file first to leverage Docker cache
COPY exam-be/WebApplication1/WebApplication1.csproj WebApplication1/

# Restore packages
RUN dotnet restore WebApplication1/WebApplication1.csproj

# Copy the entire source folder
COPY exam-be/WebApplication1/ WebApplication1/

# Publish the app
RUN dotnet publish WebApplication1/WebApplication1.csproj -c Release -o /app

# Stage 2: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS runtime
WORKDIR /app

COPY --from=build /app ./

ENV ASPNETCORE_ENVIRONMENT=Production \
    PORT=3000

EXPOSE 3000

RUN mkdir -p /app/AnimalUploads

ENTRYPOINT ["dotnet", "WebApplication1.dll"]
