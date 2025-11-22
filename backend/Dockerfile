# Etapa 1 — Build da aplicação (restaura dependências)
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia a solução
COPY *.sln .

# Copia todos os arquivos .csproj
COPY RadarProdutos.Api/RadarProdutos.Api.csproj ./RadarProdutos.Api/
COPY RadarProdutos.Application/RadarProdutos.Application.csproj ./RadarProdutos.Application/
COPY RadarProdutos.Domain/RadarProdutos.Domain.csproj ./RadarProdutos.Domain/
COPY RadarProdutos.Infrastructure/RadarProdutos.Infrastructure.csproj ./RadarProdutos.Infrastructure/

# Restaura as dependências da solução inteira
RUN dotnet restore

# Copia todo o código fonte
COPY . .

# Build da aplicação
RUN dotnet build -c Release -o /app/build

# Etapa 2 — Ambiente de desenvolvimento com Hot Reload
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS development
WORKDIR /app

# Instala dotnet-ef para migrations (versão compatível com .NET 9)
RUN dotnet tool install --global dotnet-ef --version 9.*
ENV PATH="${PATH}:/root/.dotnet/tools"

EXPOSE 5000

# Usa dotnet watch para hot reload (sem especificar --urls, usa ASPNETCORE_URLS do ambiente)
CMD ["dotnet", "watch", "run", "--project", "RadarProdutos.Api"]

# Etapa 3 — Produção
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS production
WORKDIR /app

# Copia os arquivos compilados
COPY --from=build /app/build .

EXPOSE 5000

ENTRYPOINT ["dotnet", "RadarProdutos.Api.dll"]
