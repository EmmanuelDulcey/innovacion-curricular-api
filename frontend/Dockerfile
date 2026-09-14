# Frontend Angular v1 (Issue 1). Construido sobre node:22-alpine (docker-compose).
FROM node:22-alpine

WORKDIR /app

COPY package*.json ./
RUN npm ci

COPY . .

EXPOSE 8037

# Sirve con ng serve escuchando en 0.0.0.0:8037 (script "start" en package.json).
CMD ["npm", "start"]