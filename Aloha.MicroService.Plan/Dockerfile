# Build context = thu muc goc solution (AlohaMarket.Aspire)
# Build: docker build -f Aloha.MicroService.Plan/Dockerfile -t alohamarket-plan .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Aloha.MicroService.Plan/Aloha.MicroService.Plan.csproj"
RUN dotnet publish "Aloha.MicroService.Plan/Aloha.MicroService.Plan.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aloha.MicroService.Plan.dll"]
