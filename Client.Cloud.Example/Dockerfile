FROM mcr.microsoft.com/dotnet/core/runtime:2.1 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /src
COPY ["Client.Cloud.Example.csproj", "Client.Cloud.Example/"]
RUN dotnet restore "Client.Cloud.Example/Client.Cloud.Example.csproj"
COPY . "Client.Cloud.Example"
WORKDIR "/src/Client.Cloud.Example"
RUN dotnet build "Client.Cloud.Example.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Client.Cloud.Example.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Client.Cloud.Example.dll"]
