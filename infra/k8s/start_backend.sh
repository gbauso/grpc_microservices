#!/bin/bash

# namespace
kubectl apply -f backend/namespace.yml

# # share registry secret
# kubectl get secret regcred -n default -o yaml \
# | sed s/"namespace: default"/"namespace: backend"/\
# | kubectl apply -n backend -f -

kubectl create secret generic ssl-key-cert \
--from-file=certs/server.key \
--from-file=certs/server.crt \
--from-file=certs/server.pem \
--from-file=certs/client.pem \
--from-file=certs/ca.pem \
--from-file=certs/ca.crt \
-n backend

# deployments
cat backend/deployment/weather.yml | \
sed 's/\$REG_URL'"/$REG_URL/g" | \
sed 's/\$REG_USR'"/$REG_USR/g" | \
kubectl apply -n backend -f -

cat backend/deployment/nearby-cities.yml | \
sed 's/\$REG_URL'"/$REG_URL/g" | \
sed 's/\$REG_USR'"/$REG_USR/g" | \
kubectl apply -n backend -f -

cat backend/deployment/population.yml | \
sed 's/\$REG_URL'"/$REG_URL/g" | \
sed 's/\$REG_USR'"/$REG_USR/g" | \
kubectl apply -n backend -f -

# services
kubectl apply -f backend/svc/weather-svc.yml --namespace backend
kubectl apply -f backend/svc/nearby-cities-svc.yml --namespace backend
kubectl apply -f backend/svc/population-svc.yml --namespace backend

# ingress
# kubectl apply -f backend/ingress/backend.yml --namespace backend