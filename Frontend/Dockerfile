FROM node:alpine AS build-deps
WORKDIR /usr/src/app/
COPY package.json package-lock.json  ./
RUN npm install
COPY . ./
RUN npm run build