FROM mcr.microsoft.com/dotnet/runtime-deps:5.0

WORKDIR /opt/masked-emails/inbox-api/
COPY . .

ENV ASPNETCORE_ENVIRONMENT=Production
ENV ASPNETCORE_URLS=http://+:5000
EXPOSE 5000

ENTRYPOINT ["/opt/masked-emails/inbox-api/inbox-api"]
