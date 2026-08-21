# Build the React app
FROM node:alpine AS  react
WORKDIR /app
COPY ./react/package*.json  ./
RUN npm ci
COPY ./react .
RUN npm run build

# Copy all artifacts to a volume
FROM alpine AS static
COPY --from=react app/build /static/react
COPY ./images /static/images/
COPY ./styles /static/styles/