FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY PetFeeder.API/*.csproj ./PetFeeder.API/
RUN dotnet restore PetFeeder.API/PetFeeder.API.csproj
COPY PetFeeder.API/ ./PetFeeder.API/
RUN dotnet publish PetFeeder.API/PetFeeder.API.csproj -c Release -o /app

FROM mcr.microsoft.com/dotnet/aspnet:8.0
WORKDIR /app
COPY --from=build /app .
EXPOSE 8080
ENTRYPOINT ["dotnet", "PetFeeder.API.dll"]
