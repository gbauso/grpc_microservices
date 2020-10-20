# Our first stage, that is the Builder
FROM node:12-alpine AS build
WORKDIR /app
COPY . .
RUN npm install
RUN npm run build

FROM node:12-alpine AS runtime
WORKDIR /app
COPY --from=build ./app/build ./dist
COPY --from=build ./app/contract ./dist/contract
COPY package* ./
RUN npm install --production
RUN npm install pm2 -g
CMD ["pm2-runtime","dist/index.js"]