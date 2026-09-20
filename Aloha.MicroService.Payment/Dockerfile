# Build context = thu muc goc solution (AlohaMarket.Aspire)
# Build: docker build -f Aloha.MicroService.Payment/Dockerfile -t alohamarket-payment .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Aloha.MicroService.Payment/Aloha.MicroService.Payment.csproj"
RUN dotnet publish "Aloha.MicroService.Payment/Aloha.MicroService.Payment.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aloha.MicroService.Payment.dll"]
