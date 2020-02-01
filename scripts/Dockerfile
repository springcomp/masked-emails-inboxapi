FROM ubuntu:18.04

RUN apt-get update \
 && apt-get install -y liblttng-ust0 \
 && apt-get install -y libcurl4 \
 && apt-get install -y libssl1.0.0 \
 && apt-get install -y libkrb5-3 \
 && apt-get install -y zlib1g \
 && apt-get install -y libicu60

WORKDIR /opt/masked-emails/inbox-api/
COPY . .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["/opt/masked-emails/inbox-api/inbox-api"]
