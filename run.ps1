$open_weather_id=$args[0]
$install_k3d=[bool]$args[1]
$install_istio_addons=[bool]$args[2]

if ( -not $open_weather_id ) {
    Write-Output "Error: You need to provide an open weather api key"
    exit 
}

if ( $install_k3d ) {
    $is_adm = [bool](([System.Security.Principal.WindowsIdentity]::GetCurrent()).groups -match "S-1-5-32-544")
    if( $is_adm ) {
        choco install k3d --version 3.4.0 --confirm
        k3d cluster create --api-port 6550 -p "8081:80@loadbalancer" --agents 2 --k3s-server-arg '--no-deploy=traefik' 
    }
    else {
        Write-Output "Error: You need to run it as an administrator for installing k3d"
        exit
    }
}

try {
    istioctl install --set profile=demo -y
    if ( $install_istio_addons ) {
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/jaeger.yaml
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/prometheus.yaml
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/grafana.yaml
        kubectl apply -f https://github.com/istio/istio/raw/master/samples/addons/kiali.yaml
    }
}
catch {
    Write-Output "Error: You don't have Istio or/and Kubectl installed"
}

try {
    helm upgrade --install -f ./charts/values.istio.yaml grpc charts --set weather.openWeatherId=$open_weather_id
}
catch {
    Write-Output "Error: You don't have helm charts installed"
}
