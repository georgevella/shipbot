#! /bin/bash

docker run --rm -it \
  -v "$(pwd)/appsettings.Development.json:/app/appsettings.Production.json" \
  -v "$(pwd)/azl-test-elysium.yaml:/app/azl-test-elysium.yaml" \
  georgevella/shipbot:latest-dev -a /app/azl-test-elysium.yaml
