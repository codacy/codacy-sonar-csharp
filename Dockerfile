FROM mcr.microsoft.com/dotnet/sdk:6.0 AS builder

RUN apt update && \
    apt install -y apt-transport-https dirmngr gnupg ca-certificates && \
    apt-key adv --keyserver keyserver.ubuntu.com --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    echo "deb https://download.mono-project.com/repo/debian stable-stretch main" | tee /etc/apt/sources.list.d/mono-official-stable.list && \
    apt update && \
    apt-get install -y mono-complete build-essential nuget unzip libxml2-utils

COPY . /workdir
WORKDIR /workdir

RUN make
RUN make publish
RUN make documentation

FROM ubuntu:20.04

ENV MONO_VERSION 6.12.0.122

RUN apt-get update && \
    apt-get install -y --no-install-recommends gnupg dirmngr ca-certificates && \
    rm -rf /var/lib/apt/lists/* && \
    export GNUPGHOME="$(mktemp -d)" && \
    gpg --batch --keyserver keyserver.ubuntu.com --recv-keys 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF && \
    gpg --batch --export --armor 3FA7E0328081BFF6A14DA29AA6A19B38D3D831EF > /etc/apt/trusted.gpg.d/mono.gpg.asc && \
    gpgconf --kill all && \
    rm -rf "$GNUPGHOME" && \
    apt-key list | grep Xamarin && \
    apt-get purge -y --auto-remove gnupg dirmngr

ENV TZ=Europe/Lisbon
RUN ln -snf /usr/share/zoneinfo/$TZ /etc/localtime && echo $TZ > /etc/timezone &&\
    echo "deb https://download.mono-project.com/repo/ubuntu stable-focal/snapshots/$MONO_VERSION main" > /etc/apt/sources.list.d/mono-official-stable.list && \
    apt-get update && \
    apt-get install -y -q mono-runtime mono-devel && \
    rm -rf /var/lib/apt/lists/* /tmp/*

COPY --from=builder /workdir/src/Analyzer/bin/Release/net48/publish/*dll /opt/docker/bin/
COPY --from=builder /workdir/src/Analyzer/bin/Release/net48/publish/*.exe /opt/docker/bin/
COPY --from=builder /workdir/docs /docs/

RUN adduser -u 2004 --disabled-password docker
RUN chown -R docker:docker /docs

ENTRYPOINT [ "mono", "/opt/docker/bin/Analyzer.exe" ]
