FROM mcr.microsoft.com/dotnet/aspnet:6.0
COPY . /app/out
WORKDIR /app/out
ENTRYPOINT ["dotnet", "Warehouse.Api.dll"]