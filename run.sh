#!/bin/bash

OPEN_WEATHER_ID=$1
NEW_RELIC_KEY=$2
NEW_RELIC_URL=$3
INSTALL_ISTIO_ADDONS=$4

if [ -z "$OPEN_WEATHER_ID" ]; 
then
    echo "Error: You need to provide an open weather api key"
fi

if [ -z "$NEW_RELIC_KEY" ]; 
then
    echo "Error: You need to provide a new relic api key"
fi

if [ -z "$NEW_RELIC_URL" ]; 
then
    echo "Error: You need to provide the new relic logging url"
fi


istioctl install -y

if [[ -n $INSTALL_ISTIO_ADDONS ]] && $INSTALL_ISTIO_ADDONS; 
then
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/jaeger.yaml
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/prometheus.yaml
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/grafana.yaml
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/kiali.yaml
fi

helm upgrade --install -f ./charts/values.istio.yaml grpc charts \
--set weatherService.openWeatherId=$OPEN_WEATHER_ID \
--set newRelic.apiKey=$NEW_RELIC_KEY \
--set newRelic.baseUrl=$NEW_RELIC_URL

