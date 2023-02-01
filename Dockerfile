## BUILD IMAGE
ARG DOTNET_VERSION=6.0-alpine3.17
FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION AS builder

COPY . /workdir
WORKDIR /workdir

RUN apk add --no-cache make unzip libxml2-utils &&\
    make &&\
    make publish &&\
    make documentation

## RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/runtime:$DOTNET_VERSION

COPY --from=builder /workdir/src/Analyzer/bin/Release/net6/publish/ /opt/docker/bin/
COPY --from=builder /workdir/docs /docs/

# Create NON-ROOT user
RUN adduser -u 2004 -D docker &&\
    chown -R docker:docker /docs

# From now on, run as NON-ROOT user
USER docker

ENTRYPOINT [ "dotnet", "/opt/docker/bin/Analyzer.dll" ]
