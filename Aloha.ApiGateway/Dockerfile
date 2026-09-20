# Build context = thu muc goc solution (AlohaMarket.Aspire)
# Build: docker build -f Aloha.ApiGateway/Dockerfile -t alohamarket-gateway .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Aloha.ApiGateway/Aloha.ApiGateway.csproj"
RUN dotnet publish "Aloha.ApiGateway/Aloha.ApiGateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aloha.ApiGateway.dll"]
