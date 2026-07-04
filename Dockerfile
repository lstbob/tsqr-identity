FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY --from=common . ./tsqr-common
RUN rm ./tsqr-common/nuget.config && \
    dotnet pack ./tsqr-common/src/TSQR.Common/TSQR.Common.csproj -c Release -o /tmp/nupkg

COPY nuget.config .
RUN dotnet nuget add source /tmp/nupkg --name local-common
COPY TSQR.Identity.slnx .
COPY src/TSQR.Identity.Domain/ ./src/TSQR.Identity.Domain/
COPY src/TSQR.Identity.Application/ ./src/TSQR.Identity.Application/
COPY src/TSQR.Identity.Infrastructure/ ./src/TSQR.Identity.Infrastructure/
COPY src/TSQR.Identity.Api/ ./src/TSQR.Identity.Api/

RUN dotnet restore ./src/TSQR.Identity.Api/TSQR.Identity.Api.csproj
RUN dotnet publish ./src/TSQR.Identity.Api/TSQR.Identity.Api.csproj -c Release -o /app/publish --no-restore

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS runtime
RUN apt-get update && apt-get install -y curl --no-install-recommends && rm -rf /var/lib/apt/lists/*
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://0.0.0.0:8080
ENV ASPNETCORE_ENVIRONMENT=Docker

COPY --from=build /app/publish .

ENTRYPOINT ["dotnet", "TSQR.Identity.Api.dll"]
