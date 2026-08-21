# Build the React app
FROM node:alpine AS  react
WORKDIR /app
COPY ./react/package*.json  ./
RUN npm ci
COPY ./react .
RUN npm run build

# Copy all artifacts to caddy
FROM caddy:2-alpine AS caddy
COPY --from=react app/build /srv/www/react
COPY ./images /srv/www/images/
COPY ./styles /srv/www/styles/