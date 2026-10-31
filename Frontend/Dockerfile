# Build the React app
FROM node:alpine AS  react
WORKDIR /app
COPY ./react/package*.json  ./
RUN npm ci
COPY ./react .
RUN npm run build

# Copy all artifacts to caddy
FROM caddy:2-alpine AS caddy
COPY --from=react app/build /srv/www/static/react
COPY ./images /srv/www/static/images/
COPY ./favicon /srv/www/static/favicon/
COPY ./styles /srv/www/static/styles/
COPY ./scripts /srv/www/static/scripts/