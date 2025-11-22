# 🐳 Docker Setup - Radar de Produtos

## 📋 Visão Geral

Este projeto utiliza Docker e Docker Compose para gerenciar os ambientes de desenvolvimento e produção.

## 🏗️ Estrutura

- **Backend**: ASP.NET Core 9.0 (API REST)
- **Frontend**: Next.js 16 com pnpm
- **Database**: PostgreSQL 16
- **Proxy**: Nginx (apenas produção)

## 🚀 Desenvolvimento

### Pré-requisitos

- Docker Desktop instalado
- Docker Compose v3.8+

### Comandos

```bash
# Iniciar ambiente de desenvolvimento
docker-compose -f docker-compose.dev.yml up -d

# Ver logs
docker-compose -f docker-compose.dev.yml logs -f

# Parar ambiente
docker-compose -f docker-compose.dev.yml down

# Rebuild dos containers
docker-compose -f docker-compose.dev.yml up -d --build

# Remover volumes (CUIDADO: apaga o banco de dados!)
docker-compose -f docker-compose.dev.yml down -v
```

### Acessos - Desenvolvimento

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:5000
- **PostgreSQL**: localhost:5432
  - User: `postgres`
  - Password: `postgres`
  - Database: `radar_produtos`

### Hot Reload

- ✅ **Backend**: `dotnet watch` detecta mudanças automaticamente
- ✅ **Frontend**: Next.js detecta mudanças automaticamente

### Volumes Mapeados

**Backend**:
- `./backend:/app` - Código fonte completo
- Excluídos: `bin/`, `obj/` (gerados no container)

**Frontend**:
- `./frontend:/app` - Código fonte completo
- Excluídos: `node_modules/`, `.next/` (gerados no container)

## 🏭 Produção

### Configuração

1. Copie o arquivo de exemplo:
```bash
cp .env.example .env
```

2. Edite o `.env` com suas credenciais:
```env
POSTGRES_USER=seu_usuario
POSTGRES_PASSWORD=senha_segura
NEXT_PUBLIC_API_URL=https://api.seudominio.com
```

### Comandos

```bash
# Iniciar ambiente de produção
docker-compose -f docker-compose.prod.yml up -d

# Ver logs
docker-compose -f docker-compose.prod.yml logs -f

# Parar ambiente
docker-compose -f docker-compose.prod.yml down

# Rebuild e restart
docker-compose -f docker-compose.prod.yml up -d --build
```

### Acessos - Produção

- **Nginx Proxy**: http://localhost:80
- **Frontend**: http://localhost:3000 (direto, sem proxy)
- **Backend API**: http://localhost:5000 (direto, sem proxy)
- **PostgreSQL**: localhost:5432

### Nginx (Proxy Reverso)

O Nginx está configurado para:
- Servir o frontend na rota `/`
- Rotear API para `/api/*`
- Suporte a WebSockets
- Headers de proxy corretos

Para HTTPS, edite `nginx/nginx.conf` e adicione seus certificados SSL em `nginx/ssl/`.

## 🛠️ Dockerfiles

### Backend (`backend/Dockerfile`)

**Multi-stage build**:
1. `build` - Restaura dependências e compila
2. `development` - SDK completo com dotnet watch
3. `production` - Runtime enxuto apenas com binários

**Características**:
- ✅ Copia toda a solução (.sln)
- ✅ Restaura todos os projetos (Api, Application, Domain, Infrastructure)
- ✅ Hot reload em desenvolvimento
- ✅ Build otimizado em produção
- ✅ Inclui dotnet-ef para migrations
- ✅ Usa .NET 9.0 SDK e Runtime

### Frontend (`frontend/Dockerfile`)

**Multi-stage build**:
1. `dependencies` - Instala dependências com pnpm
2. `development` - Servidor Next.js dev
3. `builder` - Build de produção
4. `production` - Servidor Next.js otimizado

**Características**:
- ✅ Usa pnpm (não npm)
- ✅ Cache de dependências otimizado
- ✅ Hot reload em desenvolvimento
- ✅ Build estático em produção

## 🔧 Troubleshooting

### Backend não compila

```bash
# Limpar volumes e rebuild
docker-compose -f docker-compose.dev.yml down -v
docker-compose -f docker-compose.dev.yml build --no-cache
docker-compose -f docker-compose.dev.yml up -d
```

### Frontend não instala dependências

```bash
# Verificar se pnpm está funcionando
docker-compose -f docker-compose.dev.yml exec frontend pnpm --version

# Reinstalar dependências
docker-compose -f docker-compose.dev.yml exec frontend pnpm install
```

### Banco de dados não conecta

```bash
# Verificar se PostgreSQL está rodando
docker-compose -f docker-compose.dev.yml ps postgres

# Ver logs do PostgreSQL
docker-compose -f docker-compose.dev.yml logs postgres

# Testar conexão
docker-compose -f docker-compose.dev.yml exec postgres psql -U postgres -d radar_produtos
```

### Hot reload não funciona

**Backend**:
- Verifique se o volume `./backend:/app` está mapeado corretamente
- O container deve estar usando o target `development`

**Frontend**:
- Verifique se o volume `./frontend:/app` está mapeado
- `node_modules` e `.next` devem estar excluídos dos volumes

## 📊 Migrations (Backend)

```bash
# Criar migration
docker-compose -f docker-compose.dev.yml exec api dotnet ef migrations add NomeDaMigration --project RadarProdutos.Infrastructure

# Aplicar migrations
docker-compose -f docker-compose.dev.yml exec api dotnet ef database update --project RadarProdutos.Infrastructure

# Reverter última migration
docker-compose -f docker-compose.dev.yml exec api dotnet ef migrations remove --project RadarProdutos.Infrastructure
```

## 🧹 Limpeza

```bash
# Remover containers parados
docker container prune

# Remover imagens não utilizadas
docker image prune -a

# Remover volumes não utilizados
docker volume prune

# Limpeza completa (CUIDADO!)
docker system prune -a --volumes
```

## 📝 Notas Importantes

1. **Development vs Production**:
   - Dev usa volumes mapeados para hot reload
   - Prod copia código para dentro da imagem

2. **Portas**:
   - Dev: PostgreSQL na 5432 (era 5433, foi corrigido)
   - Prod: Todas as portas padrão

3. **Segurança**:
   - NUNCA commit arquivos `.env` com credenciais reais
   - Use senhas fortes em produção
   - Configure HTTPS no Nginx para produção

4. **Performance**:
   - Em produção, os builds são otimizados
   - Use `.dockerignore` para excluir arquivos desnecessários
   - Multi-stage builds reduzem tamanho das imagens

## 🔗 Referências

- [ASP.NET Core Docker](https://docs.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/)
- [Next.js Docker](https://nextjs.org/docs/deployment#docker-image)
- [Docker Compose](https://docs.docker.com/compose/)
- [PostgreSQL Docker](https://hub.docker.com/_/postgres)
