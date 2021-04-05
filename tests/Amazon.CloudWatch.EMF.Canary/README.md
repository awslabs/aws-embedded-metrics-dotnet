# Running Locally

```
dotnet publish -c Release
docker build . -t dotnet-emf-canary:latest
docker run dotnet-emf-canary:latest
```