FROM alpine:3.10

RUN apk add --no-cache mono --repository http://dl-cdn.alpinelinux.org/alpine/edge/testing && \
	apk add --no-cache --virtual=.build-dependencies ca-certificates && \
	cert-sync /etc/ssl/certs/ca-certificates.crt && \
	apk del .build-dependencies \
	rm /tmp/* \
	rm -rf /var/cache/apk/*

COPY src/Analyzer/bin/Release/net461/publish/*.dll /opt/docker/bin/
COPY src/Analyzer/bin/Release/net461/publish/*.exe /opt/docker/bin/
COPY docs /docs/

RUN adduser -u 2004 -D docker
RUN chown -R docker:docker /docs

ENTRYPOINT [ "mono", "/opt/docker/bin/Analyzer.exe" ]
