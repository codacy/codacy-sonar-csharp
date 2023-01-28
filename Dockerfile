ARG DOTNET_VERSION=6.0-alpine3.17
FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS builder

COPY . /workdir
WORKDIR /workdir

RUN apk add --no-cache make unzip libxml2-utils &&\
    make &&\
    make publish &&\
    make documentation

FROM mcr.microsoft.com/dotnet/runtime:$DOTNET_VERSION

COPY --from=builder /workdir/src/Analyzer/bin/Release/net6/publish/ /opt/docker/bin/
COPY --from=builder /workdir/docs /docs/

RUN adduser -u 2004 -D docker &&\
    chown -R docker:docker /docs

ENTRYPOINT [ "dotnet", "/opt/docker/bin/Analyzer.dll" ]
