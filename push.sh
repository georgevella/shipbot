#!/usr/bin/env bash

GIT_HASH=`git rev-parse HEAD`
docker tag georgevella/shipbot:dev georgevella/shipbot:dev-${GIT_HASH:0:6} 
docker push georgevella/shipbot:dev-${GIT_HASH:0:6}

echo "Pushed image 'georgevella/shipbot:dev-${GIT_HASH:0:6}'"