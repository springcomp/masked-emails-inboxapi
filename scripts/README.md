# Overview

These scripts assume that you have compiled the application as a [self-contained](https://docs.microsoft.com/fr-fr/dotnet/core/deploying/) .NET 6.0 application. For this reason, the base image only brings the runtime dependencies that are necessary to run the application.

Compile the application using the following command-line:

```pwsh
cd src/InboxApi
dotnet publish --configuration Release --runtime linux-x64 --self-contained
```

You can compress and upload the resulting application to a blob storage in order to retrieve it from the running VPS machine.

```pwsh
Compress-Archive -Path "bin/Release/net60/linux-x64/publish/*" -DestinationPath "bin/Release/net60/archive.zip" -Force
```

or 

```bash
zip -j -9 ./bin/Release/net60/archive.zip ./bin/Release/net60/linux-x64/publish/*
```

Upload using [azcopy]():

```pwsh
azcopy login --tenant-id <tenant-id>
azcopy copy 'bin/Release/net60/archive.zip' 'https://account.blobs.core.windows.net/container/archive.zip'
```