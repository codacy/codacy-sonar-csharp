ARG DOTNET_VERSION=6.0
ARG DOTNET_BASE_OS=alpine3.20

## BUILD IMAGE
FROM mcr.microsoft.com/dotnet/sdk:$DOTNET_VERSION-$DOTNET_BASE_OS AS builder

RUN apk add --no-cache make unzip libxml2-utils

COPY . /workdir
WORKDIR /workdir

RUN make &&\
    make publish &&\
    make documentation


## RUNTIME IMAGE
FROM mcr.microsoft.com/dotnet/runtime:$DOTNET_VERSION-$DOTNET_BASE_OS

COPY --from=builder /workdir/src/Analyzer/bin/Release/net6/publish/ /opt/docker/bin/
COPY --from=builder /workdir/docs /docs/

# Create NON-ROOT user
RUN adduser -u 2004 -D docker &&\
    chown -R docker:docker /docs

# From now on, run as NON-ROOT user
USER docker

# Disable diagnostics stuff from dotnet that are turned on by default.
# Should make the image even more "read-only".
ENV DOTNET_EnableDiagnostics=0

# 16 MB stack for all runtime-created threads (value is hex); Roslyn/Sonar
# analyzers recurse over the syntax tree and overflow the ~1.5 MB default on deep code
ENV DOTNET_DefaultStackSize=1000000

ENTRYPOINT [ "dotnet", "/opt/docker/bin/Analyzer.dll" ]
