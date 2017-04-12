#!/usr/bin/env bash

SKIP_DOWNLOAD="$1"

if [ -z "$SKIP_DOWNLOAD" ]; then
    SCRIPT_DIRECTORY="$( cd "$( dirname "$0" )" && pwd )"

    RESOURCES_DIRECTORY="${SCRIPT_DIRECTORY}/../src/main/resources"

    VERSION_PATH="${SCRIPT_DIRECTORY}/../.SONAR_VERSION"

    export SONAR_VERSION="$(cat ${VERSION_PATH})"

    wget --no-check-certificate -O /tmp/sonar-csharp-plugin-"${SONAR_VERSION}".jar https://oss.sonatype.org/content/repositories/releases/org/sonarsource/dotnet/sonar-csharp-plugin/"${SONAR_VERSION}"/sonar-csharp-plugin-"${SONAR_VERSION}".jar
    unzip /tmp/sonar-csharp-plugin-"${SONAR_VERSION}".jar -d /tmp/sonar-plugin
    cp /tmp/sonar-plugin/org/sonar/plugins/csharp/rules.xml "${RESOURCES_DIRECTORY}/sonar-csharp-rules.xml"
    rm -rf /tmp/sonar-plugin /tmp/sonar-csharp-plugin-"${SONAR_VERSION}".jar
fi

sbt "run-main com.codacy.dotnet.DocGenerator"
