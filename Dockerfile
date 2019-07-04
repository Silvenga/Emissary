FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /source
COPY src/Emissary/Emissary.csproj src/Emissary/
COPY tests/Emissary.Tests/Emissary.Tests.csproj tests/Emissary.Tests/
COPY Emissary.sln .
RUN dotnet restore
COPY . .

RUN dotnet build -c Release

FROM build AS test
RUN dotnet test -c Release

FROM build AS publish
RUN dotnet publish src/Emissary/Emissary.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Emissary.dll"]
