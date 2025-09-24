# Базовый образ с Node
FROM node:20-alpine

WORKDIR /app

# Сначала package.json, чтобы кешировалась установка зависимостей
COPY package*.json ./
RUN npm install

# Копируем vite-конфиг, но не весь код (он будет монтироваться)
COPY vite.config.* ./

# Открываем порт для vite
EXPOSE 7640

CMD ["npm", "run", "dev", "--", "--host"]
