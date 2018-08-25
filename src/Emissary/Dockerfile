FROM microsoft/dotnet:2.1-runtime AS base
WORKDIR /app

FROM microsoft/dotnet:2.1-sdk AS build
WORKDIR /src
COPY src/Emissary/Emissary.csproj src/Emissary/
RUN dotnet restore src/Emissary/Emissary.csproj
COPY . .
WORKDIR /src/src/Emissary
RUN dotnet build Emissary.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish Emissary.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "Emissary.dll"]
