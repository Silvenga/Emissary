FROM mcr.microsoft.com/dotnet/core/runtime:3.1-alpine AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:3.1-alpine AS build
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
