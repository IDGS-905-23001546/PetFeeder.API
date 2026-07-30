@echo off
set SOMEE_CONNECTION_STRING=workstation id=PetFeederDB.mssql.somee.com;packet size=4096;user id=CarlosRios10_SQLLogin_1;pwd=l5mu139bcg;data source=PetFeederDB.mssql.somee.com;persist security info=False;initial catalog=PetFeederDB;TrustServerCertificate=True
dotnet run --project PetFeeder.API.csproj
pause
