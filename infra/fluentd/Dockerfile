FROM fluent/fluentd:v1.11-debian
USER root
RUN ["gem", "install", "fluent-plugin-elasticsearch-timestamp-check"]
RUN ["gem", "install", "fluent-plugin-elasticsearch", "--no-document", "--version", "4.0.9"]
USER fluent