# Sistema de Análise de Viabilidade de Produtos - Documentação Completa

## 📋 Visão Geral

Sistema completo para análise de viabilidade de produtos do AliExpress para importadores brasileiros que revendem no Mercado Livre. O sistema calcula automaticamente todos os custos (importação, taxas, frete) e sugere preços de venda com base em margens configuráveis.

## 🎯 Funcionalidades Implementadas

### Backend (.NET 9)

#### 1. **Calculadora de Viabilidade** (`ProductViabilityCalculator.cs`)

Calcula automaticamente:

- **Conversão USD → BRL** usando taxa configurável
- **Imposto de Importação** (60% sobre produto + frete)
- **Custo Total de Aquisição** (produto + frete + impostos)
- **Taxas do Mercado Livre**:
  - Taxa fixa: R$ 20,00
  - Taxa variável: 15%
  - Taxa boost: 5%
- **Impostos PJ**: 8,93% (Simples Nacional)
- **Preço de Venda Sugerido** baseado na margem alvo
- **Margem Real** e **ROI**
- **Score de Viabilidade** (0-100) ponderado por:
  - Margem de lucro
  - Volume de vendas
  - Rating do fornecedor
  - Prazo de entrega

**Fórmula do Preço Sugerido:**

```
Preço = (CustoTotal + R$20) / (1 - 15% - 5% - 8.93% - MargemAlvo%)
```

#### 2. **Entidades de Configuração**

**MarketplaceConfig** - Parâmetros totalmente configuráveis:

```csharp
- MinMarginPercent: 30%          // Margem mínima aceitável
- TargetMarginPercent: 50%       // Margem desejada
- MercadoLivreFixedFee: R$ 20    // Taxa fixa ML
- MercadoLivrePercentFee: 15%    // Taxa variável ML
- MercadoLivreBoostFee: 5%       // Taxa boost ML
- ImportTaxPercent: 60%          // Imposto importação
- CompanyTaxPercent: 8.93%       // Impostos PJ
- UsdToBrlRate: 5.70             // Cotação dólar
- AutoUpdateExchangeRate: true   // Atualizar câmbio automaticamente
- DefaultShippingCostUsd: $10    // Frete padrão
- MinSalesVolume: 100            // Mínimo de vendas
- MinSupplierRating: 4.0         // Rating mínimo
- MaxDeliveryDays: 30            // Prazo máximo entrega
- WeightMargin: 0.4              // Peso da margem no score
- WeightSales: 0.3               // Peso das vendas no score
- WeightRating: 0.2              // Peso do rating no score
- WeightDelivery: 0.1            // Peso do prazo no score
```

**ShippingEstimate** - Estimativas de frete por categoria:

```csharp
- CategoryId
- CategoryName
- MinWeight / MaxWeight / AvgWeight
- ShippingCostUsd
- EstimatedDeliveryDays
```

#### 3. **DTOs Estendidos**

**ProductDto** agora inclui **ProductViabilityDto**:

```json
{
  "viability": {
    "totalAcquisitionCost": 150.25, // Custo total de aquisição
    "suggestedSalePrice": 299.9, // Preço de venda sugerido
    "netProfit": 89.15, // Lucro líquido
    "realMarginPercent": 42.5, // Margem real
    "roi": 59.3, // Retorno sobre investimento
    "isViable": true, // Produto é viável?
    "viabilityScore": 87, // Score 0-100
    "productPriceBrl": 57.0, // Produto em BRL
    "shippingCostBrl": 28.5, // Frete em BRL
    "importTax": 51.3, // Imposto de importação
    "totalMercadoLivreFees": 60.5 // Total taxas ML
  }
}
```

#### 4. **API Endpoints**

**MarketplaceConfigController**:

```
GET  /api/marketplaceconfig       - Buscar configurações
PUT  /api/marketplaceconfig       - Atualizar configurações
```

**ProductsController** (atualizado):

```
GET  /api/products/hot           - Buscar hot products com viabilidade
     ?keyword=headphones
     &minSalePrice=10
     &maxSalePrice=100
     &pageSize=20
```

#### 5. **Banco de Dados**

**Migration criada**: `AddMarketplaceConfigurations`

- Tabela `MarketplaceConfigs` (1 registro com valores padrão)
- Tabela `ShippingEstimates` (estimativas por categoria)
- Seed data automático com valores brasileiros

#### 6. **Integração com HotProductsService**

O serviço foi atualizado para:

1. Buscar configurações do marketplace
2. Para cada produto retornado pela API:
   - Calcular viabilidade usando `ProductViabilityCalculator`
   - Mapear resultado para `ProductViabilityDto`
   - Anexar ao produto
3. Combinar score original com viability score
4. Ordenar por viabilidade (viáveis primeiro) e depois por score

### Frontend (Next.js 16)

#### 1. **Página de Configurações** (`/configuracoes`)

Interface completa para gerenciar todos os parâmetros:

**Seção: Margens de Lucro**

- Margem mínima aceitável
- Margem alvo para cálculo de preço

**Seção: Taxas e Impostos**

- Mercado Livre (fixa, variável, boost)
- Imposto de importação
- Impostos PJ

**Seção: Câmbio e Frete**

- Taxa USD → BRL
- Toggle para atualização automática
- Frete padrão
- Toggle para usar estimativas por categoria

**Seção: Filtros de Produtos**

- Volume mínimo de vendas
- Rating mínimo do fornecedor
- Prazo máximo de entrega

**Seção: Pesos do Score**

- Peso da margem (0-1)
- Peso das vendas (0-1)
- Peso do rating (0-1)
- Peso do prazo (0-1)
- Validação: total deve ser 1.0

**Funcionalidades**:

- Loading state
- Validação de pesos (soma = 1.0)
- Salvamento via API
- Toast notifications

#### 2. **Página Hot Products** (`/hot-products`)

Interface para buscar e analisar produtos em alta:

**Filtros de Busca**:

- Palavra-chave
- Faixa de preço (min/max USD)
- Tamanho da página

**Estatísticas**:

- Total de produtos encontrados
- Produtos viáveis (verde)
- Produtos inviáveis (vermelho)

**Lista de Produtos** - Cards com:

- Imagem do produto
- Nome
- Badge de viabilidade (VIÁVEL/INVIÁVEL)
- Rating e número de pedidos
- Score do produto

**Análise Detalhada por Produto**:

```
┌─────────────────────────────────────────────┐
│ Custo Total Aquisição: R$ 150.25           │
│ - Produto: R$ 57.00                        │
│ - Frete: R$ 28.50                          │
│ - Importação: R$ 51.30                     │
│                                             │
│ Preço Venda Sugerido: R$ 299.90            │
│ - Taxas ML: R$ 60.50                       │
│                                             │
│ Lucro Líquido: R$ 89.15                    │
│ - Margem: 42.5%                            │
│                                             │
│ ROI: 59.3%                                 │
│ - Score: 87/100 ████████░░                 │
└─────────────────────────────────────────────┘
```

**Cores**:

- Verde: valores positivos, produtos viáveis
- Vermelho: valores negativos, custos, produtos inviáveis
- Azul: preço sugerido
- Borda: verde para viáveis, vermelha para inviáveis

#### 3. **Sidebar Atualizado**

Novo menu item:

- 🔥 Hot Products (`/hot-products`)

## 🔧 Configuração e Execução

### Backend

```bash
cd backend/RadarProdutos.Api
dotnet run
```

Servidor roda em: `http://localhost:5001`

### Frontend

```bash
cd frontend
npm run dev
```

Aplicação roda em: `http://localhost:3000`

## 📊 Fluxo de Análise de Viabilidade

1. **Usuário configura parâmetros** em `/configuracoes`

   - Margens desejadas
   - Taxas do marketplace
   - Impostos
   - Câmbio
   - Filtros

2. **Usuário busca produtos** em `/hot-products`

   - Define palavra-chave
   - Define faixa de preço
   - Clica em "Buscar Produtos"

3. **Sistema processa**:

   - Busca produtos na API do AliExpress
   - Para cada produto:
     - Converte preço USD → BRL
     - Calcula imposto de importação (60%)
     - Soma custo total
     - Calcula taxas ML
     - Calcula preço de venda para atingir margem alvo
     - Calcula margem real e ROI
     - Gera score de viabilidade ponderado
     - Marca como viável/inviável

4. **Sistema exibe**:
   - Cards ordenados (viáveis primeiro)
   - Breakdown completo de custos
   - Preço sugerido
   - Margem e ROI
   - Score visual com barra de progresso

## 🎨 Tecnologias Utilizadas

### Backend

- .NET 9
- Entity Framework Core
- PostgreSQL 16
- Clean Architecture

### Frontend

- Next.js 16 (App Router)
- React 19
- TypeScript
- Tailwind CSS
- shadcn/ui components
- Sonner (toast notifications)

## 📝 Notas Importantes

### Permissões da API AliExpress

A API `aliexpress.affiliate.hotproduct.query` requer permissões especiais:

- Erro esperado: `"InsufficientPermission"`
- Solução: Solicitar acesso à API no painel do AliExpress
- Alternativa temporária: Usar dados mockados ou API de busca normal

### Valores Padrão (Mercado Brasileiro)

Os valores foram configurados baseados em:

- **Imposto de Importação**: 60% (produtos importados da China)
- **Taxa ML Fixa**: R$ 20,00 (valor aproximado)
- **Taxa ML Variável**: 15% (categoria geral)
- **Taxa Boost**: 5% (anúncio premium)
- **Impostos PJ**: 8,93% (Simples Nacional)
- **Câmbio**: R$ 5,70 por USD (valor de referência)
- **Margem Mínima**: 30% (viabilidade)
- **Margem Alvo**: 50% (lucro desejado)

### Fórmula Completa do Cálculo

```
1. Preço BRL = Preço USD × Taxa Câmbio
2. Frete BRL = Frete USD × Taxa Câmbio
3. Base Importação = Preço BRL + Frete BRL
4. Imposto Importação = Base × 60%
5. Custo Total = Base + Imposto
6. Taxa ML Fixa = R$ 20,00
7. Taxa ML Variável = Preço Venda × (15% + 5%)
8. Taxa ML Total = Fixa + Variável
9. Imposto PJ = (Preço Venda - Custo - Taxa ML) × 8,93%
10. Lucro Líquido = Preço Venda - Custo - Taxa ML - Imposto PJ
11. Margem Real = (Lucro / Preço Venda) × 100
12. ROI = (Lucro / Custo) × 100

Para calcular Preço Sugerido dado Margem Alvo:
Preço = (Custo + R$20) / (1 - 20% - 8.93% - MargemAlvo%)
```

### Score de Viabilidade

```
Score = (MargemScore × 0.4) +
        (VendasScore × 0.3) +
        (RatingScore × 0.2) +
        (PrazoScore × 0.1)

Onde cada score individual vai de 0-100:
- MargemScore = normalizado pela margem alvo
- VendasScore = normalizado pelo mínimo de vendas
- RatingScore = (rating / 5) × 100
- PrazoScore = inverso do prazo de entrega
```

## ✅ Checklist de Implementação

- [x] Entidade MarketplaceConfig
- [x] Entidade ShippingEstimate
- [x] ProductViabilityCalculator service
- [x] MarketplaceConfigDto
- [x] ProductViabilityDto em ProductDto
- [x] MarketplaceConfigController
- [x] MarketplaceConfigRepository
- [x] ShippingEstimateRepository
- [x] Migration AddMarketplaceConfigurations
- [x] Seed data com valores brasileiros
- [x] Integração em HotProductsService
- [x] Página de configurações frontend
- [x] Página de hot products frontend
- [x] Menu sidebar atualizado
- [x] Validação de pesos (soma = 1.0)
- [x] Loading states
- [x] Toast notifications
- [x] Responsividade
- [x] Testes de API

## 🚀 Próximos Passos Sugeridos

1. **Solicitar permissão da API** `aliexpress.affiliate.hotproduct.query`
2. **Implementar cache** de configurações para performance
3. **Adicionar gráficos** de análise de viabilidade
4. **Implementar filtros avançados** na lista de produtos
5. **Criar dashboard** com estatísticas gerais
6. **Adicionar exportação** de produtos viáveis para CSV/Excel
7. **Implementar sistema de favoritos** para produtos
8. **Criar alertas** quando novos produtos viáveis aparecerem
9. **Adicionar histórico** de análises
10. **Implementar API de cotação** automática do dólar

## 📧 Suporte

Para dúvidas sobre a implementação, consulte:

- `ARCHITECTURE.md` - Arquitetura do sistema
- `DATABASE.md` - Estrutura do banco de dados
- `CODE_REVIEW.md` - Análise de código

---

**Sistema desenvolvido para otimizar o processo de sourcing de produtos para importadores brasileiros que revendem no Mercado Livre.**
