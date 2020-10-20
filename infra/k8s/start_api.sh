#!/bin/bash

# namespace
kubectl apply -f api/namespace.yml

# # share registry secret
# kubectl get secret regcred -n default -o yaml \
# | sed s/"namespace: default"/"namespace: api"/\
# | kubectl apply -n api -f -

kubectl create secret generic ssl-key-cert \
--from-file=certs/server.key \
--from-file=certs/server.crt \
--from-file=certs/server.pem \
--from-file=certs/client.pem \
--from-file=certs/ca.pem \
--from-file=certs/ca.crt \
-n api

# deployments
cat api/deployment/api-gateway.yml | \
sed 's/\$REG_URL'"/$REG_URL/g" | \
sed 's/\$REG_USR'"/$REG_USR/g" | \
kubectl apply -n api -f -

# services
kubectl apply -f api/svc/api-gateway-svc.yml --namespace api

# ingress
kubectl apply -f api/ingress/api.yml --namespace api