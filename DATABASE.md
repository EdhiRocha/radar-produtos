# Configuração do Banco de Dados PostgreSQL

## Opção 1: Usando Docker (Recomendado)

### Iniciar PostgreSQL com Docker Compose:

```powershell
docker-compose up -d
```

### Parar o container:

```powershell
docker-compose down
```

### Ver logs:

```powershell
docker-compose logs -f postgres
```

## Opção 2: PostgreSQL Local

Se você já tem PostgreSQL instalado localmente, apenas crie o banco:

```sql
CREATE DATABASE radar_produtos;
```

E ajuste a connection string em `backend/RadarProdutos.Api/appsettings.json`.

## Aplicar Migrations

As migrations são aplicadas automaticamente quando a aplicação inicia. Mas você pode aplicar manualmente:

```powershell
cd backend\RadarProdutos.Api
dotnet ef database update
```

## Connection String

Padrão (Docker):

```
Host=localhost;Port=5432;Database=radar_produtos;Username=postgres;Password=postgres
```

**⚠️ IMPORTANTE:** Em produção, use variáveis de ambiente e senhas seguras!

## Estrutura do Banco

### Tabelas:

- **Users** - Usuários do sistema
- **Plans** - Planos de assinatura (Free, Trial, Pro)
- **Subscriptions** - Assinaturas ativas dos usuários
- **ProductAnalysis** - Análises executadas
- **Products** - Produtos encontrados em cada análise
- **AnalysisConfigs** - Configurações de cálculo de score

### Planos Pré-configurados:

| Plano | Preço    | Buscas/Mês | Buscas/Dia | Filtros Avançados |
| ----- | -------- | ---------- | ---------- | ----------------- |
| Free  | R$ 0     | 10         | 3          | ❌                |
| Trial | R$ 0     | 30         | 10         | ✅                |
| Pro   | R$ 47,90 | Ilimitado  | Ilimitado  | ✅                |
