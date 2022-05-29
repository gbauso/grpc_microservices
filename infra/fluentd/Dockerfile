FROM fluent/fluentd:v1.9.2-1.0
ENV LOG_LEVEL="warn"
ENV BASE_URI="https://log-api.newrelic.com/log/v1"

USER root

RUN apk add --no-cache --update --virtual .build-deps \
        sudo build-base ruby-dev \
 && sudo fluent-gem install fluent-plugin-newrelic \
 && sudo gem sources --clear-all \
 && apk del .build-deps \
 && rm -rf /home/fluent/.gem/ruby/*/cache/*.gem

COPY conf/fluent.conf /fluentd/etc/
COPY entrypoint.sh /bin/

USER fluent