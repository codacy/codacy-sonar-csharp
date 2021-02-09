FROM mcr.microsoft.com/dotnet/core/sdk:2.2 AS builder

RUN apt update && \
    apt install -y apt-transport-https dirmngr gnupg ca-certificates && \
    apt-key adv --keyserver hkp://keyserver.ubuntu.com:80 --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb https://download.mono-project.com/repo/debian stable-stretch main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
    apt update && \
    apt-get install -y mono-complete build-essential nuget unzip libxml2-utils

COPY . /workdir
WORKDIR /workdir

RUN make
RUN make publish

FROM mono:6.10

COPY --from=builder /workdir/src/Analyzer/bin/Release/net461/publish/*dll /opt/docker/bin/
COPY --from=builder /workdir/src/Analyzer/bin/Release/net461/publish/*.exe /opt/docker/bin/
COPY docs /docs/

RUN adduser -u 2004 --disabled-password docker
RUN chown -R docker:docker /docs

ENTRYPOINT [ "mono", "/opt/docker/bin/Analyzer.exe" ]
