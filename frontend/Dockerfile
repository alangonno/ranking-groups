# =========================================
# Stage 1: Development
# =========================================
FROM node:24-alpine AS dev

WORKDIR /app

# Copy package files first for layer caching
COPY package.json package-lock.json* ./

# Install dependencies
RUN --mount=type=cache,target=/root/.npm \
    npm install

# Copy source code
COPY . .

# Expose Vite dev server port
EXPOSE 5173

CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

# =========================================
# Stage 2: Build
# =========================================
FROM node:24-alpine AS build

WORKDIR /app

COPY package.json package-lock.json* ./

RUN --mount=type=cache,target=/root/.npm \
    npm ci

COPY . .

RUN npm run build
