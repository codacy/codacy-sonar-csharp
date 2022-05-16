FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

COPY . /workdir
WORKDIR /workdir

RUN apt update && apt install -y make unzip libxml2-utils
RUN make
RUN make publish
RUN make documentation

FROM mcr.microsoft.com/dotnet/runtime:6.0

COPY --from=builder /workdir/src/Analyzer/bin/Release/net6/publish/ /opt/docker/bin/
COPY --from=builder /workdir/docs /docs/

RUN adduser -u 2004 --disabled-password docker
RUN chown -R docker:docker /docs

ENTRYPOINT [ "dotnet", "/opt/docker/bin/Analyzer.dll" ]
