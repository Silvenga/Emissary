FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /source
COPY src/Emissary/Emissary.csproj src/Emissary/
COPY tests/Emissary.Tests/Emissary.Tests.csproj tests/Emissary.Tests/
COPY Emissary.sln .
RUN dotnet restore
COPY . .

ARG BUILD_VERSION=0.0.1
RUN dotnet build -c Release -p:Version=${BUILD_VERSION}

FROM build AS test
ARG BUILD_VERSION=0.0.1
RUN dotnet test -c Release -p:Version=${BUILD_VERSION}

FROM build AS publish
ARG BUILD_VERSION=0.0.1
RUN dotnet publish src/Emissary/Emissary.csproj -c Release -o /app -p:Version=${BUILD_VERSION}

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Emissary.dll"]
