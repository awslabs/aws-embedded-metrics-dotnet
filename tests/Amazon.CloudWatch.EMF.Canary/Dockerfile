FROM mcr.microsoft.com/dotnet/aspnet:8.0
# Apply latest Debian security updates to clear out-of-SLA OS-package CVEs
RUN apt-get update && apt-get -y upgrade && rm -rf /var/lib/apt/lists/*
COPY bin/Release/net8.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "Amazon.CloudWatch.EMF.Canary.dll"]
