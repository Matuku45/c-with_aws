# Use official .NET SDK image for build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy solution and project files
COPY *.sln ./
COPY myWEBAplication/*.csproj ./myWEBAplication/

# Restore dependencies explicitly for the solution
RUN dotnet restore myWEBAplication.sln

# Copy all source code to container
COPY myWEBAplication/. ./myWEBAplication/

# Set working directory to project folder
WORKDIR /src/myWEBAplication

# Publish the app (release mode, no app host)
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Use runtime-only image for smaller size
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app

# Copy published files from build stage
COPY --from=build /app/publish .

# Expose port (change if your app listens on a different port)
EXPOSE 80

# Start the app
ENTRYPOINT ["dotnet", "myWEBAplication.dll"]
