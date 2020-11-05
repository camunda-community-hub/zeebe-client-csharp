FROM mcr.microsoft.com/dotnet/core/runtime:2.1 AS runtime

# copy pre-build example
COPY bin/Debug/netcoreapp2.1/ app/

WORKDIR /app

# RUN example
ENTRYPOINT ["dotnet", "Client.Examples.dll"]
