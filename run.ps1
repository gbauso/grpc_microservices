$open_weather_id=$args[0]
$new_relic_url=$args[1]
$new_relic_key=$args[2]
$install_istio_addons=[bool]$args[3]

if ( -not $open_weather_id ) {
    Write-Output "Error: You need to provide an open weather api key"
    exit 
}

if ( -not $new_relic_url ) {
    Write-Output "Error: You need to provide the new relic logging url"
    exit 
}

if ( -not $new_relic_key ) {
    Write-Output "Error: You need to provide a new relic api key"
    exit 
}

try {
    istioctl install -y
    if ( $install_istio_addons ) {
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/jaeger.yaml
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/prometheus.yaml
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/grafana.yaml
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/kiali.yaml
    }
}
catch {
    Write-Output "Error: You don't have Istio or/and Kubectl installed"
    exit
}

try {
    helm upgrade --install -f ./charts/values.istio.yaml grpc charts \
    --set weatherService.openWeatherId=$open_weather_id \
    --set newRelic.apiKey=$new_relic_key \
    --set newRelic.baseUrl=$new_relic_url
}
catch {
    Write-Output "Error: You don't have helm charts installed"
}
