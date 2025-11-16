# 🔍 Radar Produtos

Sistema de análise de produtos do AliExpress para identificar oportunidades de revenda com dados em tempo real.

## 📋 Sobre o Projeto

Radar Produtos é uma plataforma SaaS que ajuda empreendedores a encontrar produtos lucrativos no AliExpress através de:

- ✅ Análise inteligente de relevância (filtro de 40%)
- ✅ Cálculo automático de margens de lucro
- ✅ Avaliação de competitividade
- ✅ Busca por tendências (produtos em alta)
- ✅ Persistência de dados com PostgreSQL
- ✅ Arquitetura multi-tenant preparada para escalar

## 🏗️ Arquitetura

### Backend (.NET 9)

- **Clean Architecture** com separação em camadas:
  - `RadarProdutos.Api` - API REST
  - `RadarProdutos.Application` - Lógica de negócio
  - `RadarProdutos.Domain` - Entidades e contratos
  - `RadarProdutos.Infrastructure` - Acesso a dados e serviços externos

### Frontend (Next.js 16)

- React 19 com TypeScript
- Tailwind CSS para estilização
- shadcn/ui para componentes
- App Router (Next.js)

### Banco de Dados

- PostgreSQL 16 (containerizado)
- Entity Framework Core 8.0
- Migrações versionadas

## 🚀 Funcionalidades

### Análise de Produtos

- Busca por palavra-chave (ex: "smart watch")
- Busca sem filtro para "produtos em alta"
- Filtro de relevância de 40% baseado em palavras-chave
- Cálculo de margem de lucro (preço sugerido vs custo)
- Score de competitividade

### Gestão de Dados

- Persistência de análises no PostgreSQL
- Histórico de buscas
- Produtos salvos para comparação

### Multi-Tenant (Preparado)

- Sistema de planos: Free, Trial, Pro
- Limites de buscas por mês/dia
- Controle de uso por usuário

## 🗄️ Modelo de Dados

### Entidades Principais

**Users**

- Autenticação e controle de acesso
- Relacionamento 1:1 com Subscription

**Plans**

- Free: 10 buscas/mês
- Trial: 30 buscas/mês
- Pro: Ilimitado (R$ 47,90/mês)

**Subscriptions**

- Plano ativo do usuário
- Contadores de uso (mensal/diário)
- Data de renovação

**ProductAnalysis**

- Armazena cada análise realizada
- Relacionamento 1:N com Products

**Products**

- Dados do produto (nome, preço, imagem)
- Score e margem calculados
- Relacionamento N:1 com ProductAnalysis

## 🛠️ Tecnologias

### Backend

- .NET 9
- Entity Framework Core 8.0
- Npgsql (PostgreSQL provider)
- Swagger/OpenAPI

### Frontend

- Next.js 16.0.3
- React 19
- TypeScript
- Tailwind CSS
- shadcn/ui

### Infraestrutura

- Docker & Docker Compose
- PostgreSQL 16-alpine
- Git

## 📦 Instalação

### Pré-requisitos

- .NET 9 SDK
- Node.js 18+
- Docker & Docker Compose
- Git

### 1. Clonar o Repositório

```bash
git clone https://github.com/seu-usuario/radar-produtos.git
cd radar-produtos
```

### 2. Configurar Banco de Dados

```bash
# Iniciar container PostgreSQL
docker-compose up -d

# Aplicar migrações
cd backend/RadarProdutos.Api
dotnet ef database update
```

### 3. Configurar Backend

```bash
cd backend/RadarProdutos.Api

# Instalar dependências (automático no dotnet restore)
dotnet restore

# Executar
dotnet run
```

Backend estará disponível em: `http://localhost:5000`

### 4. Configurar Frontend

```bash
cd frontend

# Instalar dependências
npm install

# Executar em modo desenvolvimento
npm run dev
```

Frontend estará disponível em: `http://localhost:3000`

## 🔧 Configuração

### Backend (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5433;Database=radar_produtos;Username=postgres;Password=postgres"
  },
  "AliExpressApi": {
    "AppKey": "521918",
    "AppSecret": "SUA_SECRET_AQUI"
  }
}
```

### Frontend (lib/api.ts)

```typescript
const API_URL = "http://localhost:5000";
```

### Docker (docker-compose.yml)

- PostgreSQL rodando na porta `5433` (evita conflito com outras instâncias)
- Volume `postgres_data` para persistência
- Healthcheck configurado

## 📊 Estrutura do Banco

```sql
-- Tabelas criadas pela migração InitialCreate
Users (Id, Email, PasswordHash, Name)
Plans (Id, Name, PriceMonthly, MaxSearchesPerMonth, MaxSearchesPerDay)
Subscriptions (Id, UserId, PlanId, SearchesUsedThisMonth, SearchesUsedToday)
ProductAnalysis (Id, Keyword, CreatedAt, UserId)
Products (Id, ExternalId, Name, Price, ImageUrl, ProductAnalysisId)
AnalysisConfigs (Id, UserId, MinMargin, MinScore)
```

## 🎯 Planos Disponíveis

| Plano | Buscas/Mês | Buscas/Dia | Preço    | Recursos           |
| ----- | ---------- | ---------- | -------- | ------------------ |
| Free  | 10         | -          | Grátis   | Básico             |
| Trial | 30         | -          | Grátis   | Avaliação 14 dias  |
| Pro   | Ilimitado  | Ilimitado  | R$ 47,90 | Completo + Suporte |

## 📝 API Endpoints

### Análise

- `POST /api/analysis/run` - Executar análise de produtos
  ```json
  {
    "keyword": "smart watch",
    "config": {
      "minimumMarginPercent": 30,
      "minimumScore": 7
    }
  }
  ```

### Produtos

- `GET /api/products` - Listar produtos salvos
- `GET /api/products/{id}` - Detalhes do produto

## 🧪 Testes

```bash
# Backend
cd backend
dotnet test

# Frontend
cd frontend
npm test
```

## 📈 Roadmap

- [ ] Implementar autenticação JWT
- [ ] Dashboard com gráficos de análise
- [ ] Exportação de relatórios PDF/Excel
- [ ] Integração com outros marketplaces
- [ ] Sistema de notificações
- [ ] API pública para integrações

## 🤝 Contribuindo

1. Fork o projeto
2. Crie uma branch para sua feature (`git checkout -b feature/MinhaFeature`)
3. Commit suas mudanças (`git commit -m 'Adiciona MinhaFeature'`)
4. Push para a branch (`git push origin feature/MinhaFeature`)
5. Abra um Pull Request

## 📄 Licença

Este projeto está sob a licença MIT. Veja o arquivo `LICENSE` para mais detalhes.

## 👥 Autores

- Desenvolvedor Principal - [@seu-usuario](https://github.com/seu-usuario)

## 🆘 Suporte

Para questões e suporte:

- Abra uma [Issue](https://github.com/seu-usuario/radar-produtos/issues)
- Email: suporte@radarprodutos.com

## 🙏 Agradecimentos

- AliExpress API pela integração
- Comunidade .NET e Next.js
- Todos os contribuidores

---

**Radar Produtos** - Encontre as melhores oportunidades de revenda 🚀
