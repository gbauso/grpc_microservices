FROM adoptopenjdk/openjdk14:x86_64-debianslim-jre14u-nightly as base

ENV PORT=80
WORKDIR /app
EXPOSE $PORT

FROM adoptopenjdk/openjdk14:x86_64-debian-jdk14u-nightly-slim as build

WORKDIR /app
COPY . .
RUN chmod +x ./gradlew
RUN ./gradlew installDist

FROM base as final

COPY --from=build /app/build/install/weather-service .
ENTRYPOINT ./bin/weather-server