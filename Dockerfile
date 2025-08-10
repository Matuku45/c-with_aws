# Use the official .NET SDK image to build the app
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy csproj and restore as distinct layers
COPY *.sln .
COPY myWEBAplication/*.csproj ./myWEBAplication/
RUN dotnet restore

# Copy everything and build
COPY myWEBAplication/. ./myWEBAplication/
WORKDIR /src/myWEBAplication
RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Build runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "myWEBAplication.dll"]
