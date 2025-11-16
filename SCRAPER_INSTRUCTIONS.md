# Radar de Produtos – Microserviço Scraper (Node/NestJS)

## ⚠️ Instruções para criar o microserviço

O backend .NET está **pronto e rodando**, mas ele precisa consumir um microserviço de scraping em Node/NestJS que ainda não foi criado.

Para criar o microserviço de scraping, **use o Prompt 2** fornecido no seu pedido original.

### Resumo do que o Prompt 2 vai gerar:

- **Aplicação NestJS** chamada `radar-scraper`
- **Porta**: 4000
- **Endpoints**:
  - `GET /scraper/products?keyword=...` → Retorna lista de produtos mockados
  - `GET /scraper/competition?name=...` → Retorna informações de concorrência
  - `GET /scraper/engagement?name=...` → Retorna sentimento e engajamento

### Próximos passos:

1. **Abra o Copilot em uma nova pasta** ou workspace separado
2. **Cole o Prompt 2** completo (fornecido no seu pedido original)
3. O Copilot criará toda a estrutura NestJS com:
   - Controllers
   - Services
   - DTOs
   - Dados mockados
4. Execute `npm install` e `npm run start:dev`
5. O scraper estará disponível em `http://localhost:4000`

### Integração

Uma vez que o microserviço Node estiver rodando:

- O backend .NET automaticamente chamará os endpoints do scraper
- Você poderá testar o endpoint `POST /api/analysis/run` com `{ "keyword": "smartphone" }`
- O .NET consumirá os dados mockados do Node e calculará os scores

---

**Backend .NET**: ✅ Pronto e rodando em `http://localhost:5000`  
**Scraper Node**: ⏳ Aguardando criação (use Prompt 2)
