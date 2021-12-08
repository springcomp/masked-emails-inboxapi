#!/usr/bin/env bash
unzip -j archive.zip -d ./context/
docker build -t "springcomp/masked-emails-inbox-api:v0.8" -f Dockerfile ./context/
docker tag "springcomp/masked-emails-inbox-api:v0.8" "springcomp/masked-emails-inbox-api:latest"