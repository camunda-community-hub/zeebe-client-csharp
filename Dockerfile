FROM mcr.microsoft.com/dotnet/core/sdk:2.1 AS build
WORKDIR /app

# copy csproj and restore as distinct layers
COPY ./Client Client/
COPY ./docs/ docs/
COPY ./Client.Examples Client.Examples/
WORKDIR /app/Client.Examples/

RUN dotnet restore *.csproj
RUN dotnet publish -c release -o out --no-restore

FROM mcr.microsoft.com/dotnet/core/runtime:2.1 AS runtime
# copy pre-build example
COPY --from=build /app/Client.Examples/out app/

# COPY bin/Debug/netcoreapp2.1/ app/

WORKDIR /app

# RUN example
ENTRYPOINT ["dotnet", "Client.Examples.dll"]
