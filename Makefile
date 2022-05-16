SONAR_VERSION=8.39.0.47922

BUILD_CMD=dotnet build --no-restore /property:GenerateFullPaths=true

RESOURCE_FOLDER=.res

all: configure build

configure:
	dotnet restore

build-all:
	$(BUILD_CMD)

build:
	$(BUILD_CMD) src/Analyzer

build-docs:
	$(BUILD_CMD) src/DocsGenerator

update-docs:
	curl -L "https://github.com/SonarSource/sonar-dotnet/releases/download/$(SONAR_VERSION)/sonar-csharp-plugin-$(SONAR_VERSION).jar" \
		-o sonar-csharp-plugin.jar
	mkdir -p $(RESOURCE_FOLDER)
	unzip sonar-csharp-plugin.jar -d '$(RESOURCE_FOLDER)/sonar-csharp-plugin'
	cp $(RESOURCE_FOLDER)/sonar-csharp-plugin/org/sonar/plugins/csharp/rules.xml "$(RESOURCE_FOLDER)/sonar-csharp-rules.xml"
	rm sonar-csharp-plugin.jar
	rm -rf $(RESOURCE_FOLDER)/sonar-csharp-plugin/

documentation: update-docs build-docs

documentation:
	echo $(SONAR_VERSION) > .SONAR_VERSION
	dotnet "src/DocsGenerator/bin/Debug/net6/DocsGenerator.dll"
	rm .SONAR_VERSION

publish:
	dotnet publish -c Release -f net6

clean:
	rm -rf .lib/ .packages/ .res/
