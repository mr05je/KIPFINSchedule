FROM mcr.microsoft.com/dotnet/aspnet:7.0 AS base
WORKDIR /app
EXPOSE 9011

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["KIPFINSchedule.Api/KIPFINSchedule.Api.csproj", "KIPFINSchedule.Api/"]
COPY ["KIPFINSchedule.Core/KIPFINSchedule.Core.csproj", "KIPFINSchedule.Core/"]
COPY ["KIPFINSchedule.Database/KIPFINSchedule.Database.csproj", "KIPFINSchedule.Database/"]
RUN dotnet restore "KIPFINSchedule.Api/KIPFINSchedule.Api.csproj"
COPY . .
WORKDIR "/src/KIPFINSchedule.Api"
RUN dotnet build "KIPFINSchedule.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "KIPFINSchedule.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
RUN chmod 777 /app
ENTRYPOINT ["dotnet", "KIPFINSchedule.Api.dll"]
