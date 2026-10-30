# syntax=docker/dockerfile:1
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY Directory.Build.props Directory.Packages.props ./
COPY src/TodoApp.Domain/TodoApp.Domain.csproj src/TodoApp.Domain/
COPY src/TodoApp.Application/TodoApp.Application.csproj src/TodoApp.Application/
COPY src/TodoApp.Infrastructure/TodoApp.Infrastructure.csproj src/TodoApp.Infrastructure/
COPY src/TodoApp.ServiceDefaults/TodoApp.ServiceDefaults.csproj src/TodoApp.ServiceDefaults/
COPY src/TodoApp.Api/TodoApp.Api.csproj src/TodoApp.Api/

RUN dotnet restore src/TodoApp.Api/TodoApp.Api.csproj

COPY src/TodoApp.Domain/ src/TodoApp.Domain/
COPY src/TodoApp.Application/ src/TodoApp.Application/
COPY src/TodoApp.Infrastructure/ src/TodoApp.Infrastructure/
COPY src/TodoApp.ServiceDefaults/ src/TodoApp.ServiceDefaults/
COPY src/TodoApp.Api/ src/TodoApp.Api/

RUN dotnet publish src/TodoApp.Api/TodoApp.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_HTTP_PORTS=8080
EXPOSE 8080

COPY --from=build /app/publish .

USER $APP_UID
ENTRYPOINT ["dotnet", "TodoApp.Api.dll"]
