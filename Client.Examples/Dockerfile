FROM mcr.microsoft.com/dotnet/core/runtime:3.1 AS runtime

# copy pre-build example
COPY bin/Debug/netcoreapp3.1/ app/

WORKDIR /app

# RUN example
ENTRYPOINT ["dotnet", "Client.Examples.dll"]
