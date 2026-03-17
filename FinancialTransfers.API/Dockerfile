FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["FinancialTransfers.API/FinancialTransfers.API.csproj", "FinancialTransfers.API/"]
COPY ["FinancialTransfers.Application/FinancialTransfers.Application.csproj", "FinancialTransfers.Application/"]
COPY ["FinancialTransfers.Domain/FinancialTransfers.Domain.csproj", "FinancialTransfers.Domain/"]
COPY ["FinancialTransfers.Infraestructure/FinancialTransfers.Infraestructure.csproj", "FinancialTransfers.Infraestructure/"]
RUN dotnet restore "FinancialTransfers.API/FinancialTransfers.API.csproj"
COPY . .
WORKDIR "/src/FinancialTransfers.API"
RUN dotnet build "FinancialTransfers.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "FinancialTransfers.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FinancialTransfers.API.dll"]