#!/bin/bash

# namespace
kubectl apply -f infra/namespace.yml

kubectl create secret generic ssl-key-cert \
--from-file=certs/server.key \
--from-file=certs/server.crt \
--from-file=certs/server.pem \
--from-file=certs/client.pem \
--from-file=certs/ca.pem \
--from-file=certs/ca.crt \
-n infra

# deployments
kubectl apply -f infra/deployment/elk.yml --namespace infra
kubectl apply -f infra/deployment/etcd.yml --namespace infra
kubectl apply -f infra/deployment/rabbitmq.yml --namespace infra

cat infra/deployment/discovery.yml | \
sed 's/\$REG_URL'"/$REG_URL/g" | \
sed 's/\$REG_USR'"/$REG_USR/g" | \
kubectl apply -n infra -f -

# services
kubectl apply -f infra/svc/elk-svc.yml --namespace infra
kubectl apply -f infra/svc/etcd-svc.yml --namespace infra
kubectl apply -f infra/svc/etcd-ui-svc.yml --namespace infra
kubectl apply -f infra/svc/rabbitmq-svc.yml --namespace infra

# ingress
kubectl apply -f infra/ingress/infra.yml --namespace infra