#!/bin/bash

docker volume create local_registry
docker container run -d --name registry.local -v local_registry:/var/lib/registry --restart always -p 5002:5000 registry:2

k3d create --workers 2 --publish 8081:80 --image docker.io/rancher/k3s:latest -server-arg "--no-deploy=traefik" \
--enable-registry --enable-registry-cache --registry-volume=local_registry

sleep 10

docker network connect k3d-k3s-default registry.local
export KUBECONFIG="$(k3d get-kubeconfig --name='k3s-default')"

docker network connect k3d-k3s-default registry.local