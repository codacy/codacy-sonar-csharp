FROM develar/java

RUN \
	apk add --no-cache bash wget unzip && \
	apk add --no-cache --virtual=.build-dependencies ca-certificates tar xz && \
	wget "https://www.archlinux.org/packages/extra/x86_64/mono/download/" -O "/tmp/mono.pkg.tar.xz" && \
	tar -xJf "/tmp/mono.pkg.tar.xz" && \
	cert-sync /etc/ssl/certs/ca-certificates.crt && \
	apk del .build-dependencies && \
	rm /tmp/*