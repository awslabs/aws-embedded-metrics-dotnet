FROM mcr.microsoft.com/dotnet/core/aspnet:6.0
COPY bin/Release/net6.0/publish/ App/
WORKDIR /App
ENTRYPOINT ["dotnet", "Amazon.CloudWatch.EMF.Canary.dll"]