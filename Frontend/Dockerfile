# Build the react app
FROM node:alpine AS build-deps
WORKDIR /usr/src/app/
COPY ./React/package.json ./React/package-lock.json ./React ./
RUN npm install
COPY . ./
RUN npm run build

FROM nginx:1-alpine
COPY --from=build-deps /usr/src/app/build /usr/share/nginx/html
COPY ./images /usr/share/nginx/html/images
COPY ./styles /usr/share/nginx/html/styles
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]