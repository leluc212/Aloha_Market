# Build context = thu muc goc solution (AlohaMarket.Aspire)
# Build: docker build -f Aloha.CategoryService/Dockerfile -t alohamarket-category .
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY . .
RUN dotnet restore "Aloha.CategoryService/Aloha.MicroService.Category.csproj"
RUN dotnet publish "Aloha.CategoryService/Aloha.MicroService.Category.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
ENV ASPNETCORE_URLS=http://+:8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Aloha.MicroService.Category.dll"]
