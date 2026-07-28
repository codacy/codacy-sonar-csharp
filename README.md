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
  - dotnet-sdk-6.0 - "The .NET Core SDK"
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

## Agent Playbook: Updating This Repository End-to-End

This section is written for an AI coding agent (or a human) tasked with updating this repo — most commonly bumping the wrapped SonarC# (SonarAnalyzer.CSharp) version, but also base image / orb / NuGet dependency bumps. Follow it top to bottom; it tells you what to change, how to regenerate derived files, how to test locally, and how to interpret CI so you can iterate on failures without guessing.

### 1. What this repository is

This is a **Codacy engine**: a .NET (C#, `net6`) wrapper (`src/Analyzer`, built on the `Codacy.Engine.Seed` NuGet package) that packages [SonarC#](https://github.com/SonarSource/sonar-dotnet) (SonarSource's `SonarAnalyzer.CSharp` Roslyn analyzer) as a Docker image Codacy's platform can run against a customer's source code. The build/orchestration layer is a `Makefile` invoked from a multi-stage `Dockerfile` (no Scala/sbt here, and no Python/Node tooling — everything is `dotnet` CLI + `make`).

The `docs/` directory is machine-consumed configuration, just like in the Scala-based Codacy engines:

- `docs/patterns.json` — the full list of SonarC# rules ("patterns") Codacy knows about, their parameters/defaults, and which are enabled by default. Generated file, do not hand-edit.
- `docs/description/description.json` + `docs/description/*.md` — human-readable titles/descriptions per pattern, used in the Codacy UI. Generated file, do not hand-edit.
- `docs/tests/*` and `docs/multiple-tests/*` — fixtures used by `codacy-plugins-test` to validate the engine actually produces the results it claims to for real code samples.
- `docs/tool-description.md` — short blurb about the tool, hand-maintained.

The generator is **`src/DocsGenerator`** (a separate .NET console project, `src/DocsGenerator/Program.cs`). It does **not** clone a GitHub repo like the Scala engines' `DocGenerator` does — instead, `make update-docs` downloads the actual SonarSource release **JAR** (`sonar-csharp-plugin-<version>.jar`) from `https://github.com/SonarSource/sonar-dotnet/releases/download/<version>/sonar-csharp-plugin-<version>.jar`, unzips it, and extracts `Rules.json` plus each rule's `S*.json`/`S*.html` files into `.res/`. `DocsGenerator` then reads those extracted files, plus a `.SONAR_VERSION` marker file, and writes `docs/patterns.json` and `docs/description/*`. This means regenerating docs needs **network access** to GitHub, plus `unzip` and `xmllint` (`libxml2-utils`) installed locally — no `pandoc` needed.

### 2. Files that encode versions — check all of these on every update

| File | What it controls | What to check |
|---|---|---|
| `src/Analyzer/Analyzer.csproj` → `SonarAnalyzer.CSharp` PackageReference `Version` | The wrapped SonarC# analyzer version. This is also read by `Makefile`'s `SONAR_VERSION` var (via `xmllint`) to know which release JAR/tag to fetch for doc regeneration. | Bump to the target version (format `<major>.<minor>.<build>.<revision>`, e.g. `9.32.0.97167`). Confirm a matching GitHub release tag exists at [SonarSource/sonar-dotnet releases](https://github.com/SonarSource/sonar-dotnet/releases). |
| `src/Analyzer/Analyzer.csproj` → `Microsoft.CodeAnalysis.CSharp.Workspaces`, `SQLitePCLRaw.core`, `Codacy.Engine.Seed` | Roslyn workspace API and other NuGet deps | Bump independently if asked (dependabot also opens PRs for these); not tied to SonarC# bumps. |
| `docs/patterns.json` → `"version"` field | The version string Codacy displays for this tool | Regenerated automatically by `DocsGenerator` — do not hand-edit; just verify it matches after regeneration. |
| `.circleci/config.yml` → `codacy/base` orb | Shared CircleCI steps (checkout, versioning, docker build/publish, tagging) | Check the latest published version in the CircleCI orb registry (`git log -p .circleci/config.yml` shows prior bump history as a fallback reference). |
| `.circleci/config.yml` → `codacy/plugins-test` orb | Runs `codacy-plugins-test` in CI after the image is built | Same as above. |
| `Dockerfile` → `ARG DOTNET_VERSION` / `ARG DOTNET_BASE_OS` | .NET SDK/runtime version and Alpine base used to build and run the analyzer | Only bump if the new SonarAnalyzer.CSharp version raises its minimum .NET requirement, or if asked explicitly — don't bump opportunistically. |

### 3. Step-by-step update procedure

1. **Bump `SonarAnalyzer.CSharp` (and any other targeted dependency)** in `src/Analyzer/Analyzer.csproj`, and `.circleci/config.yml` orbs if in scope.
2. **Regenerate the docs.** Requires `unzip` and `xmllint` on `PATH`, plus network access to GitHub: `make documentation` (this runs `update-docs` — download + extract the release JAR — followed by `build-docs` and the `DocsGenerator` run). Review the diff in `docs/patterns.json` and `docs/description/*` for new/removed/renamed rules and stale fixture references.
3. **Restore and build:** `make configure && make build-all` (or just `make build` for the Analyzer only).
4. **Build the Docker image**: `docker build -t codacy-sonar-csharp:local .`
5. **Run `codacy-plugins-test` locally** before pushing — clone https://github.com/codacy/codacy-plugins-test and run the relevant DockerTest commands (this repo's CI passes `run_multiple_tests: true`, i.e. it exercises the `multiple` tests in `docs/multiple-tests/` in addition to the standard pattern/json tests) against your local image tag.
6. **Iterate on failures**, re-running only the relevant DockerTest command after each fix.
7. **Commit** the version bump(s) together with the regenerated `docs/` files in one change.
8. **Push and open a PR.** CI runs `checkout_and_version` -> `publish_local` (docker build) -> `plugins_test` -> `publish_docker` (master only) -> `tag_version`.
9. **Poll the PR's real CI checks until they all pass — local validation is NOT the finish line.** After every push, run `gh pr checks <pr-url>` and keep re-polling (short sleep while any check is `pending`) until all checks finish. If a check fails, fetch its actual log (CircleCI API/UI for the failing job — don't guess), find the true root cause, fix it, push again (never `--no-verify`, never force-push), and re-poll. Repeat until every check is green. **The CI environment's toolchain can differ from your local one**, so a clean local run does not guarantee CI passes. Only stop iterating when every check passes, or you hit a genuine product/infra decision that needs a human — in which case explain it in the PR rather than guessing.

### 4. Common failure modes and fixes

| Symptom | Likely cause | Fix |
|---|---|---|
| `make update-docs` fails to download the JAR | Wrong/nonexistent version string, or the release asset naming changed upstream | Verify the exact tag and asset filename on the [sonar-dotnet releases page](https://github.com/SonarSource/sonar-dotnet/releases) |
| `pattern`/`json` DockerTest fails | Rule renamed/removed/added upstream between versions | Re-run `make documentation`; confirm the change matches the upstream changelog for that SonarC# release |
| `multiple` DockerTest fails on a specific fixture folder | Expectations in `docs/multiple-tests/` stale for new analyzer behavior | Regenerate/update the expected results to match the new (verified correct) output |
| Docker build fails after a .NET bump | `TargetFrameworks` in `.csproj` files not updated to match the new `DOTNET_VERSION` base image | Keep `Analyzer.csproj`/`DocsGenerator.csproj` `TargetFrameworks` and the Dockerfile's `DOTNET_VERSION` in sync |
| CI `publish_docker`/`tag_version` don't run on your branch | Expected — gated to the default branch (`master`) only | Nothing to fix |

### 5. Definition of done

- Version bump(s) reflected in `src/Analyzer/Analyzer.csproj` (and `.circleci/config.yml` orbs, if in scope).
- Generated docs (`docs/patterns.json`, `docs/description/*`) regenerated via `make documentation` and committed, with fixture inconsistencies resolved.
- Local `make configure && make build-all` succeeds.
- Docker image builds successfully (`docker build -t codacy-sonar-csharp:local .`).
- `codacy-plugins-test` DockerTest commands all pass locally against the freshly built image.
- **After pushing and opening/updating the PR, every CI check on it is green.** Poll `gh pr checks <pr-url>` and iterate on any failure (fetch the real CI log, fix, push, re-poll) until all pass — a passing local build is not sufficient, because the CI toolchain can differ from your local one (see step 9).

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
