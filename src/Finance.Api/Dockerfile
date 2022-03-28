FROM mcr.microsoft.com/dotnet/aspnet:6.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["Finance.Api/Finance.Api.fsproj", "Finance.Api/"]
RUN dotnet restore "Finance.Api/Finance.Api.fsproj"
COPY . .
WORKDIR "/src/Finance.Api"
RUN dotnet build "Finance.Api.fsproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Finance.Api.fsproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Finance.Api.dll"]
