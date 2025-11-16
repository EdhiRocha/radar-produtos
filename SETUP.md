# 🚀 Setup Completo - Radar de Produtos

## 1️⃣ Iniciar o Banco de Dados PostgreSQL

```powershell
# Com Docker (Recomendado)
docker-compose up -d

# Verificar se está rodando
docker ps
```

Aguarde ~10 segundos para o PostgreSQL inicializar.

## 2️⃣ Iniciar o Backend (.NET)

```powershell
cd backend\RadarProdutos.Api
dotnet run
```

**✅ O backend vai:**

- Conectar ao PostgreSQL
- Aplicar migrations automaticamente
- Criar tabelas (Users, Plans, Subscriptions, Products, etc.)
- Fazer seed dos planos (Free, Trial, Pro)
- Iniciar na porta 5001

## 3️⃣ Iniciar o Frontend (Next.js)

Em outro terminal:

```powershell
cd frontend
pnpm dev
```

**✅ O frontend vai iniciar em:** http://localhost:3000

## 🎯 Testar o Sistema

1. Abra http://localhost:3000
2. Faça uma busca (ex: "smart watch") ou deixe vazio para produtos em alta
3. Veja os produtos filtrados pela relevância
4. Clique em um produto para ver detalhes

## 🔍 Verificar o Banco de Dados

### Via linha de comando:

```powershell
# Conectar ao PostgreSQL
docker exec -it radar-produtos-db psql -U postgres -d radar_produtos

# Listar tabelas
\dt

# Ver planos criados
SELECT * FROM "Plans";

# Ver produtos salvos
SELECT "Id", "Name", "SupplierPrice", "Score" FROM "Products" LIMIT 5;

# Sair
\q
```

### Via ferramenta gráfica:

- **pgAdmin**, **DBeaver**, ou **DataGrip**
- Host: localhost
- Port: 5432
- Database: radar_produtos
- Username: postgres
- Password: postgres

## 📊 Status do Sistema

### ✅ Implementado:

- PostgreSQL + EF Core
- Multi-tenant (Users, Plans, Subscriptions)
- Migrations automáticas
- Persistência de produtos e análises
- 3 planos pré-configurados (Free, Trial, Pro)
- API AliExpress com filtro de relevância
- Interface completa com filtros

### 🔄 Próximos Passos:

- Autenticação JWT
- Middleware de rate limiting por plano
- Sistema de registro/login
- Dashboard com métricas reais do banco

## 🛠️ Comandos Úteis

### Backend:

```powershell
# Criar nova migration
cd backend\RadarProdutos.Api
dotnet ef migrations add NomeDaMigration --project ..\RadarProdutos.Infrastructure

# Aplicar migrations
dotnet ef database update

# Reverter migration
dotnet ef migrations remove
```

### Docker:

```powershell
# Parar PostgreSQL
docker-compose down

# Ver logs
docker-compose logs -f postgres

# Resetar banco (CUIDADO: apaga tudo!)
docker-compose down -v
docker-compose up -d
```

## ⚠️ Troubleshooting

**Backend não conecta no PostgreSQL:**

- Verifique se o container está rodando: `docker ps`
- Verifique a connection string em `appsettings.json`

**Erro de migration:**

- Delete a pasta `Migrations` e recrie: `dotnet ef migrations add InitialCreate`

**Porta 5432 já em uso:**

- Pare outros PostgreSQL: `docker stop $(docker ps -aq)`
- Ou mude a porta no `docker-compose.yml`
