# ============================================================
# FRONTEND (React/Vite) — build Node → nginx no-root
# ============================================================
FROM node:22-alpine AS build
WORKDIR /app
ARG VITE_API_URL=/api
COPY SecureDevOps.Web/package.json SecureDevOps.Web/package-lock.json* ./
RUN npm ci
COPY SecureDevOps.Web/ ./
ENV VITE_API_URL=$VITE_API_URL
RUN npm run build

FROM nginxinc/nginx-unprivileged:1.27-alpine
COPY --from=build /app/dist /usr/share/nginx/html
COPY SecureDevOps.Web/nginx.conf /etc/nginx/conf.d/default.conf
EXPOSE 8080
CMD ["nginx", "-g", "daemon off;"]