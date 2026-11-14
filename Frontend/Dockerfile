# Build the React app
FROM node:alpine AS  react
WORKDIR /app
COPY ./react/package*.json  ./
RUN npm ci
COPY ./react .
RUN npm run build

# Copy all artifacts 
FROM alpine:latest AS pack
WORKDIR /app/dist
COPY --from=react app/build ./react
COPY ./images ./images/
COPY ./favicon ./favicon/
COPY ./styles ./styles/

CMD ["cp", "-r", "/app/dist/.", "/output"]