# Build stage
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /App
COPY CertStore.Domain ./CertStore.Domain/
COPY CertStore.Infrastructure ./CertStore.Infrastructure/
COPY CertStore.API ./CertStore.API/
COPY CertStore.slnx ./

RUN dotnet restore CertStore.slnx
RUN dotnet publish CertStore.API -o out

# Runtime stage
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /App
COPY --from=build /App/out .
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000
ENTRYPOINT ["dotnet", "CertStore.API.dll"]
