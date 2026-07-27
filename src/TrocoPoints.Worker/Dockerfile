FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY TrocoPoints.slnx .
COPY src/TrocoPoints.Domain/TrocoPoints.Domain.csproj src/TrocoPoints.Domain/
COPY src/TrocoPoints.Application/TrocoPoints.Application.csproj src/TrocoPoints.Application/
COPY src/TrocoPoints.Infrastructure/TrocoPoints.Infrastructure.csproj src/TrocoPoints.Infrastructure/
COPY src/TrocoPoints.Worker/TrocoPoints.Worker.csproj src/TrocoPoints.Worker/
RUN dotnet restore src/TrocoPoints.Worker/TrocoPoints.Worker.csproj

COPY src/TrocoPoints.Domain/ src/TrocoPoints.Domain/
COPY src/TrocoPoints.Application/ src/TrocoPoints.Application/
COPY src/TrocoPoints.Infrastructure/ src/TrocoPoints.Infrastructure/
COPY src/TrocoPoints.Worker/ src/TrocoPoints.Worker/

RUN dotnet publish src/TrocoPoints.Worker/TrocoPoints.Worker.csproj -c Release -o /app --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
WORKDIR /app
COPY --from=build /app .

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

ENTRYPOINT ["dotnet", "TrocoPoints.Worker.dll"]
