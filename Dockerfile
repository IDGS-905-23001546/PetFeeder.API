FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PetFeeder.API.csproj .
RUN dotnet restore PetFeeder.API.csproj
COPY . .
RUN dotnet publish PetFeeder.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PetFeeder.API.dll"]
