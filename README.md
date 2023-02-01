[![Codacy Badge](https://api.codacy.com/project/badge/Grade/ff929008ec754fe790738a9a15821f93)](https://www.codacy.com/gh/codacy/codacy-sonar-csharp?utm_source=github.com&utm_medium=referral&utm_content=codacy/codacy-sonar-csharp&utm_campaign=Badge_Grade)
[![Build Status](https://circleci.com/gh/codacy/codacy-sonar-csharp.svg?style=shield&circle-token=:circle-token)](https://circleci.com/gh/codacy/codacy-sonar-csharp)

# Codacy SonarC#

This is the docker engine we use at Codacy to run [SonarC#](https://github.com/SonarSource/sonar-csharp) developed by SonarSource.

You can also create a docker to integrate the tool and language of your choice!
Check the **Docs** section for more information.

## Local Development

**Requirements**:
  - unzip 
  - xmllint
    * on ubuntu: `apt-get install libxml2-utils`
    * on alpine: `apk add libxml2-utils`
  - dotnet-sdk - "The .NET Core SDK"
    * on archlinux: the above package also installs `dotnet-runtime`, `dotnet-host` and `dotnet-targeting-pack`) - the .NET Core SDK

### IDE

This seems to be more or less working in vscode, install the 
"C# for Visual Studio Code (powered by OmniSharp)" extension 
and before opening the project in it do `make configure`.

### Commands

  - `make configure` - runs `dotnet restore` which downloads all the required libraries for the projects to work.
  - `make build` - compiles the *Analyzer* project.
  - `make build-docs` - compiles the *DocsGenerator* project.
  - `make build-all` - compiles both the *Analyzer* and *DocsGenerator* projects.
  - `make documentation` - downloads upstream rules for the *sonar version* we defined in `Analyzer.csproj`,
    extracts the rules for that version and runs the *DocsGenerator* application.

See other useful targets inside the `Makefile`.

## Usage

### Publish the docker image locally

```bash
docker build -t codacy-sonar-csharp:local .
```

### Run the docker locally

```bash
docker run --user=docker --rm -v <PATH-TO-CODE>:/src:ro -v <PATH-TO>/.codacyrc:/.codacyrc:ro codacy-sonar-csharp:local
```

### Enter inside the docker image

```bash
docker run --user=docker --rm -v <PATH-TO-CODE>:/src:ro -v <PATH-TO>/.codacyrc:/.codacyrc:ro -it --entrypoint /bin/sh codacy-sonar-csharp:local
```

> Make sure all the volumes mounted have the right permissions for user `docker`

## Tool configuration file

Currently, to use your own configuration file, you must add a SonarLint.xml xml file with an AnalysisInput structure inside.

Example:
```
    <?xml version="1.0" encoding="UTF-8"?>
    <AnalysisInput>
      <Rules>
        <Rule>
          <Key>S103</Key>
          <Parameters>
            <Parameter>
              <Key>maximumLineLength</Key>  
              <Value>24</Value>  
            </Parameter>
          </Parameters>
        </Rule>
      </Rules>
    </AnalysisInput>
```

## Docs

[Tool Developer Guide](https://support.codacy.com/hc/en-us/articles/207994725-Tool-Developer-Guide)

[Tool Developer Guide - Using Scala](https://support.codacy.com/hc/en-us/articles/207280379-Tool-Developer-Guide-Using-Scala)

## Test

We use the [codacy-plugins-test](https://github.com/codacy/codacy-plugins-test) to test our external tools integration.
You can follow the instructions there to make sure your tool is working as expected.

## What is Codacy?

[Codacy](https://www.codacy.com/) is an Automated Code Review Tool that monitors your technical debt, helps you improve your code quality, teaches best practices to your developers, and helps you save time in Code Reviews.

### Among Codacy’s features:

-   Identify new Static Analysis issues
-   Commit and Pull Request Analysis with GitHub, BitBucket/Stash, GitLab (and also direct git repositories)
-   Auto-comments on Commits and Pull Requests
-   Integrations with Slack, HipChat, Jira, YouTrack
-   Track issues in Code Style, Security, Error Proneness, Performance, Unused Code and other categories

Codacy also helps keep track of Code Coverage, Code Duplication, and Code Complexity.

Codacy supports PHP, Python, Ruby, Java, JavaScript, and Scala, among others.

### Free for Open Source

Codacy is free for Open Source projects.
