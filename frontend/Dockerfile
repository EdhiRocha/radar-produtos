# Etapa 1 — Dependências
FROM node:22-alpine AS dependencies

WORKDIR /app

# Instala pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

# Copia arquivos de dependências
COPY package.json pnpm-lock.yaml ./

# Instala dependências
RUN pnpm install --frozen-lockfile

# Etapa 2 — Desenvolvimento
FROM node:22-alpine AS development

WORKDIR /app

# Instala pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

EXPOSE 3000

# Next.js dev server
CMD ["pnpm", "dev"]

# Etapa 3 — Builder (para produção)
FROM node:22-alpine AS builder

WORKDIR /app

# Instala pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

# Copia dependências instaladas
COPY --from=dependencies /app/node_modules ./node_modules
COPY . .

# Build da aplicação
RUN pnpm build

# Etapa 4 — Produção
FROM node:22-alpine AS production

WORKDIR /app

# Instala pnpm
RUN corepack enable && corepack prepare pnpm@latest --activate

ENV NODE_ENV=production

# Copia arquivos necessários
COPY --from=builder /app/next.config.mjs ./
COPY --from=builder /app/public ./public
COPY --from=builder /app/.next ./.next
COPY --from=builder /app/node_modules ./node_modules
COPY --from=builder /app/package.json ./

EXPOSE 3000

CMD ["pnpm", "start"]
