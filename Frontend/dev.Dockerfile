# Build the React app
FROM node:alpine AS  react
WORKDIR /app
COPY ./react/package*.json  ./
RUN npm ci
COPY ./react .
RUN npm run build

# Copy images to caddy server
FROM caddy:2-alpine
COPY ./Caddy/dev/Caddyfile /etc/caddy/Caddyfile
COPY --from=react /app/build /srv/www/react/
COPY ./images /srv/www/images/
COPY ./styles /srv/www/styles/