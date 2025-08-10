# Use official Microsoft .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copy the solution file first
COPY myWEBAplication.sln ./

# Copy project folder and csproj file(s)
COPY myWEBAplication/ ./myWEBAplication/

# Restore dependencies for the solution
RUN dotnet restore myWEBAplication.sln

# Copy everything else (source code)
COPY . .

# Build and publish the app
WORKDIR /src/myWEBAplication
RUN dotnet publish -c Release -o /app/publish

# Use runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0
WORKDIR /app
COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "myWEBAplication.dll"]
