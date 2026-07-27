FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TrocoPoints.slnx .
COPY src/TrocoPoints.Domain/TrocoPoints.Domain.csproj src/TrocoPoints.Domain/
COPY src/TrocoPoints.Application/TrocoPoints.Application.csproj src/TrocoPoints.Application/
COPY src/TrocoPoints.Infrastructure/TrocoPoints.Infrastructure.csproj src/TrocoPoints.Infrastructure/
COPY src/TrocoPoints.Api/TrocoPoints.Api.csproj src/TrocoPoints.Api/
RUN dotnet restore src/TrocoPoints.Api/TrocoPoints.Api.csproj

COPY src/TrocoPoints.Domain/ src/TrocoPoints.Domain/
COPY src/TrocoPoints.Application/ src/TrocoPoints.Application/
COPY src/TrocoPoints.Infrastructure/ src/TrocoPoints.Infrastructure/
COPY src/TrocoPoints.Api/ src/TrocoPoints.Api/

RUN dotnet publish src/TrocoPoints.Api/TrocoPoints.Api.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TrocoPoints.Api.dll"]
