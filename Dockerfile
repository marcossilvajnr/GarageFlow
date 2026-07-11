FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/GarageFlow.Api/GarageFlow.Api.csproj", "src/GarageFlow.Api/"]
COPY ["src/GarageFlow.Application/GarageFlow.Application.csproj", "src/GarageFlow.Application/"]
COPY ["src/GarageFlow.Infrastructure/GarageFlow.Infrastructure.csproj", "src/GarageFlow.Infrastructure/"]
COPY ["src/GarageFlow.Domain/GarageFlow.Domain.csproj", "src/GarageFlow.Domain/"]
COPY ["src/GarageFlow.WebHost/GarageFlow.WebHost.csproj", "src/GarageFlow.WebHost/"]
RUN dotnet restore "src/GarageFlow.WebHost/GarageFlow.WebHost.csproj"

COPY . .
RUN dotnet publish "src/GarageFlow.WebHost/GarageFlow.WebHost.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080

COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "GarageFlow.WebHost.dll"]
