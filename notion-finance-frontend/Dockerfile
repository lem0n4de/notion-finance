# develop stage
FROM node:14.18-alpine as develop-stage
RUN apk add --update --no-cache curl py-pip python3 make gcc g++ 
WORKDIR /app
COPY package*.json ./
RUN yarn install
COPY . .
# build stage
FROM develop-stage as build-stage
RUN yarn build --fix --mode development
# production stage
FROM nginx:mainline-alpine as production-stage
COPY --from=build-stage /app/dist /usr/share/nginx/html
COPY nginx.conf nginx.conf
COPY nginx.conf /temp/prod.conf
RUN envsubst /app < /temp/prod.conf > /etc/nginx/conf.d/default.conf
EXPOSE 80
CMD ["nginx", "-c", "nginx.conf"]