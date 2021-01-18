#!/bin/bash

OPEN_WEATHER_ID=$1
INSTALL_ISTIO_ADDONS=$2

if [ -z "$OPEN_WEATHER_ID" ]; 
then
    echo "Error: You need to provide an open weather api key"
fi

istioctl install -y

if [[ -n $INSTALL_ISTIO_ADDONS ]] && $INSTALL_ISTIO_ADDONS; 
then
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/jaeger.yaml
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/prometheus.yaml
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/grafana.yaml
    kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/kiali.yaml
fi

helm upgrade --install -f ./charts/values.istio.yaml grpc charts --set weatherService.openWeatherId=$OPEN_WEATHER_ID

