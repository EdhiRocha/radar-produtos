# Arquitetura da Solução Radar de Produtos

## Visão Geral

Sistema de análise de produtos para dropshipping composto por:

- **Backend Principal**: .NET 9 Web API
- **Scraper Service**: Node.js/NestJS (microserviço)
- **Persistência**: Entity Framework Core InMemory
- **Front-end**: Next.js/React (já existente)

## Fluxo de Dados

```
[Front-end Next.js]
        ↓
[API .NET :5000]
        ↓
[ScraperHttpClient] → [Scraper Service :4000]
        ↓                    ↓
[AnalysisService]     [Dados mockados]
        ↓
[ProductScoreCalculator]
        ↓
[Repositórios + EF Core]
        ↓
[Banco InMemory]
```

## Camadas do Backend .NET

### 1. Domain (RadarProdutos.Domain)

**Responsabilidade**: Entidades de negócio e contratos

**Entidades**:

- `Product`: Produto analisado com score calculado
- `ProductAnalysis`: Agregador de uma análise (contém múltiplos produtos)
- `AnalysisConfig`: Configurações de pesos e margens

**Interfaces**:

- `IProductRepository`
- `IProductAnalysisRepository`
- `IAnalysisConfigRepository`
- `IScraperClient`

**DTOs de Integração**:

- `ScrapedProductDto`: Produto retornado pelo scraper
- `CompetitionInfoDto`: Dados de concorrência
- `EngagementInfoDto`: Sentimento e engajamento

### 2. Application (RadarProdutos.Application)

**Responsabilidade**: Orquestração de casos de uso

**Serviços**:

- `AnalysisService`: Coordena todo o processo de análise
  - Chama scraper para obter produtos
  - Chama scraper para concorrência/engajamento de cada produto
  - Calcula score usando `ProductScoreCalculator`
  - Persiste análise e produtos
- `ProductScoreCalculator`: Lógica de cálculo de score
  - Normaliza vendas (orders)
  - Pondera concorrência (Baixa/Media/Alta)
  - Pondera sentimento (Positivo/Negativo/Misto)
  - Normaliza margem
  - Combina usando pesos configuráveis

**DTOs Públicos**:

- `ProductDto`
- `ProductAnalysisDto`
- `AnalysisConfigDto`

**Requests**:

- `RunAnalysisRequest`

### 3. Infrastructure (RadarProdutos.Infrastructure)

**Responsabilidade**: Implementações concretas

**Data**:

- `AppDbContext`: Contexto EF Core com InMemory database

**Repositórios**:

- `ProductRepository`: CRUD de produtos com filtros
- `ProductAnalysisRepository`: CRUD de análises com Include de produtos
- `AnalysisConfigRepository`: Singleton de configuração

**Integração**:

- `ScraperHttpClient`: Cliente HTTP tipado
  - Implementa `IScraperClient`
  - Lê `Scraper:BaseUrl` de configuração
  - Deserializa JSON responses do Node

### 4. Api (RadarProdutos.Api)

**Responsabilidade**: Exposição HTTP

**Controllers**:

- `AnalysisController`:

  - `POST /api/analysis/run`: Dispara análise
  - `GET /api/analysis/latest`: Última análise

- `ProductsController`:

  - `GET /api/products`: Lista paginada com filtros
  - `GET /api/products/{id}`: Detalhes de um produto

- `ConfigController`:
  - `GET /api/config`: Retorna configuração atual
  - `PUT /api/config`: Atualiza pesos e margens

**Configuração (Program.cs)**:

- DI de repositórios, serviços e HttpClient
- EF Core InMemory
- Swagger habilitado
- CORS aberto
- Seed de configuração padrão

## Scraper Service (Node/NestJS)

**Estrutura**:

```
ScraperService/
├── src/
│   ├── scraper/
│   │   ├── scraper.controller.ts      # Endpoints REST
│   │   ├── scraper.module.ts          # Módulo NestJS
│   │   ├── products-scraper.service.ts
│   │   ├── competition-scraper.service.ts
│   │   ├── engagement-scraper.service.ts
│   │   └── dtos.ts
│   ├── app.module.ts
│   └── main.ts                         # Bootstrap (porta 4000)
```

**Endpoints**:

- `GET /scraper/products?keyword=X`: Retorna 5-10 produtos mockados
- `GET /scraper/competition?name=X`: Retorna concorrência mock
- `GET /scraper/engagement?name=X`: Retorna sentimento mock

**Mocks**:

- Dados gerados aleatoriamente
- Estrutura pronta para plugar scrapers reais futuramente

## Fluxo de Análise Completo

1. **Front-end** envia `POST /api/analysis/run { "keyword": "smartphone" }`

2. **AnalysisController** recebe e delega para `AnalysisService`

3. **AnalysisService.RunAnalysisAsync**:
   - Chama `IScraperClient.GetProductsFromSupplierAsync("smartphone")`
4. **ScraperHttpClient** faz `GET http://localhost:4000/scraper/products?keyword=smartphone`

5. **Scraper Service** retorna lista de `ScrapedProductDto` (mock)

6. **AnalysisService** para cada produto:

   - Chama `GetCompetitionInfoAsync(productName)`
   - Chama `GetEngagementInfoAsync(productName)`
   - Monta entidade `Product` com todos os dados
   - Chama `ProductScoreCalculator.CalculateScore(...)`
   - Adiciona à lista

7. **AnalysisService** persiste:

   - Cria `ProductAnalysis` com keyword e timestamp
   - Associa produtos à análise
   - Salva via `IProductAnalysisRepository` e `IProductRepository`

8. **AnalysisService** retorna lista de `ProductDto` para o controller

9. **AnalysisController** retorna JSON ao front-end

## Cálculo de Score

Fórmula simplificada:

```
score = (w1 * vendas_norm) +
        (w2 * concorrencia_norm) +
        (w3 * sentimento_norm) +
        (w4 * margem_norm)
```

Normalizações:

- **Vendas**: Linear entre 0-1000 orders
- **Concorrência**: Baixa=1.0, Media=0.5, Alta=0.0
- **Sentimento**: Positivo=1.0, Misto=0.5, Negativo=0.0
- **Margem**: Linear entre MinMargin-MaxMargin configurado

Resultado: 0-100

## Extensibilidade

### Para adicionar scrapers reais:

**No ScraperService**:

1. Instalar bibliotecas: `puppeteer`, `cheerio`, `axios`
2. Implementar lógica real em cada service
3. Manter mesma interface de DTOs

**Exemplo**:

```typescript
// products-scraper.service.ts
async getProductsByKeyword(keyword: string): Promise<ScrapedProductDto[]> {
  // Substituir mock por:
  const browser = await puppeteer.launch();
  // ... scraping real do AliExpress
  return productsFromAliExpress;
}
```

### Para adicionar novo fator de score:

1. Adicionar propriedade em `Product` (Domain)
2. Criar DTO correspondente no Scraper
3. Atualizar `ProductScoreCalculator`
4. Adicionar peso em `AnalysisConfig`

## Tecnologias

| Camada       | Stack                                    |
| ------------ | ---------------------------------------- |
| API Backend  | .NET 9, ASP.NET Core, C#                 |
| Persistência | EF Core 9.0, InMemory                    |
| Scraper      | Node.js, NestJS, TypeScript              |
| Documentação | Swagger/OpenAPI                          |
| DI           | Microsoft.Extensions.DependencyInjection |
| HTTP Client  | HttpClient com Typed Clients             |

## Próximos Passos

- [ ] Substituir InMemory por SQL Server/PostgreSQL
- [ ] Implementar scrapers reais (AliExpress, ML, YouTube)
- [ ] Adicionar autenticação (JWT)
- [ ] Rate limiting no scraper
- [ ] Cache de resultados
- [ ] Logs estruturados (Serilog)
- [ ] Health checks
- [ ] Testes unitários e de integração
