#! /bin/bash

helm upgrade shipbot . --namespace argo -f ./values-linting.yaml --tls