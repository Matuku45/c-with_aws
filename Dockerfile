FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copy the solution file and project files
COPY *.sln ./
COPY myWEBAplication/*.csproj ./myWEBAplication/

# Restore using the solution file explicitly
RUN dotnet restore myWEBAplication.sln

# Copy everything else and build
COPY myWEBAplication/. ./myWEBAplication/
WORKDIR /src/myWEBAplication

RUN dotnet publish -c Release -o /app/publish /p:UseAppHost=false

# Runtime image
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime
WORKDIR /app
COPY --from=build /app/publish .
EXPOSE 80
ENTRYPOINT ["dotnet", "myWEBAplication.dll"]
