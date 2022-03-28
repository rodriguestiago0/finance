FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Finance.Service/Finance.Service.fsproj", "Finance.Service/"]
RUN dotnet restore "Finance.Service/Finance.Service.fsproj"
COPY . .
WORKDIR "/src/Finance.Service"
RUN dotnet build "Finance.Service.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Finance.Service.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Finance.Service.dll"]
