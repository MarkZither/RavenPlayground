FROM microsoft/dotnet:2.1-aspnetcore-runtime-nanoserver-1709 AS base
WORKDIR /app
EXPOSE 58751
EXPOSE 44347

FROM microsoft/dotnet:2.1-sdk-nanoserver-1709 AS build
WORKDIR /src
COPY RavenPlayground.Web/RavenPlayground.Web.csproj RavenPlayground.Web/
RUN dotnet restore RavenPlayground.Web/RavenPlayground.Web.csproj
COPY . .
WORKDIR /src/RavenPlayground.Web
RUN dotnet build RavenPlayground.Web.csproj -c Release -o /app

FROM build AS publish
RUN dotnet publish RavenPlayground.Web.csproj -c Release -o /app

FROM base AS final
WORKDIR /app
COPY --from=publish /app .
ENTRYPOINT ["dotnet", "RavenPlayground.Web.dll"]
