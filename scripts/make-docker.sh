#!/usr/bin/env bash
unzip -j archive.zip -d ./context/
docker build -t springcomp/masked-emails-inbox-api:latest -f Dockerfile ./context/
