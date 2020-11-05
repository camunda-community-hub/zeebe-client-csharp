#!/bin/bash

## Expects that dotnet, docker and kubectl is installed and that a correct k8 context is set to deploy the example
set -exuo pipefail

## Builds the project
cd ../
dotnet restore *.sln
dotnet build *.sln

## Build image
cd Client.Examples/
docker build -t gcr.io/zeebe-io/zeebe-client-csharp-example:latest .

## Publish image
docker push gcr.io/zeebe-io/zeebe-client-csharp-example:latest

## Deploy
kubectl apply -f example.yaml
