# RestSharp – Best Practices

This document captures practical guidance for developing, testing, reviewing, and releasing changes in the RestSharp repository. It consolidates conventions used across the solution and provides checklists and examples to reduce regressions and ensure consistency across target frameworks.


## 1) Daily Development Workflow

- Prefer small, focused pull requests with clear descriptions and test coverage.
- Build and test locally before pushing. Validate all relevant target frameworks (TFMs) for your changes.
- Keep commits tidy; prefer meaningful commit messages over large "fix" commits.
- Follow repository code style and file organization rules (see sections below).
- Use the latest C# language features (C# 14).

Suggested loop:
- dotnet build
- dotnet test

## 2) Multi-Targeting Guidance

The core library targets netstandard2.0, net471, net48, net8.0, net9.0, and net10.0. Tests target net48 (Windows only), net8.0, net9.0, and net10.0.

- Use conditional compilation for TFM-specific APIs or behaviors:
  - #if NET
  - #if NET for platform attributes (e.g., [UnsupportedOSPlatform("browser")])
- When adding features, ensure compilation succeeds for all TFMs. If an API is missing on older TFMs, add polyfills or conditional code, or guard with feature detection.
- Be mindful of System.Text.Json: it is a package dependency on older TFMs but built-in on modern TFMs.
- Validate tests for each TFM impacted by your changes. If a test only applies to modern .NET, guard it with #if NET8_0_OR_GREATER.


## 3) Build and Configuration

- Shared build logic lives in props/Common.props, src/Directory.Build.props, and test/Directory.Build.props. Do not duplicate MSBuild settings in individual projects unless necessary.
- Language version is preview; prefer modern C# features but ensure cross-TFM compatibility.
- Nullable reference types:
  - Enabled in /src (Nullable=enable). Treat nullable warnings as design feedback.
  - Disabled in /test (Nullable=disable) for test authoring ergonomics.
- Assemblies are strong-named via RestSharp.snk. Do not remove signing.
- Package versions are centrally managed in Directory.Packages.props; do not pin versions locally unless justified by TFM constraints.


## 4) Source Generators

Custom incremental generators live in gen/SourceGenerator and are referenced as analyzers by src/RestSharp.

- Use [GenerateImmutable] to produce immutable wrappers of mutable classes.
  - Exclude properties with [Exclude] when not needed in the immutable type.
  - Generated files are emitted to obj/<Configuration>/<TFM>/generated/SourceGenerator/.
- Use [GenerateClone(BaseType=..., Name=...)] to create static factory methods that upcast/copy base properties into derived types.
- When editing generators:
  - Keep the generator targets netstandard2.0.
  - Ensure EmitCompilerGeneratedFiles is enabled when debugging locally.
  - Validate output by building and inspecting generated files under obj/...
- Keep generator helpers cohesive (Extensions.cs) and prefer small, composable utilities.


## 5) Testing Strategy

- Frameworks and libraries: xUnit + FluentAssertions + AutoFixture.
- Organization:
  - Unit tests in test/RestSharp.Tests.
  - Integration tests (HTTP/WireMock) in test/RestSharp.Tests.Integrated.
  - Serializer-specific tests in dedicated projects.
  - Shared helpers in test/RestSharp.Tests.Shared.
- Best practices:
  - Co-locate tests with feature areas; use partial classes to split large suites.
  - Prefer WireMockServer over live endpoints for HTTP scenarios.
  - Avoid flaky tests: don’t depend on timing, locale, or network conditions.
  - Use descriptive assertions, e.g., result.Should().Be(expected).
  - Scope tests by TFM when API availability differs.
- Useful commands:
  - dotnet test RestSharp.sln -c Debug
  - dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0
  - dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "FullyQualifiedName=Namespace.Class.Method" -f net8.0
- Test results are written to test-results/<TFM>/<ProjectName>.trx.


## 6) Code Coverage

- Use coverlet.collector for data-collector-based coverage.
- Example command (Cobertura output):
  - dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net10.0 --collect:"XPlat Code Coverage" -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
- Coverage artifacts are placed alongside test results in test-results.


## 7) Dependency Injection and Extensions

- DI extensions live in src/RestSharp.Extensions.DependencyInjection.
- When updating DI helpers:
  - Keep APIs minimal and idiomatic for Microsoft.Extensions.DependencyInjection.
  - Maintain backward compatibility where feasible; add overloads rather than breaking changes.
  - Add focused tests under test/RestSharp.Tests.DependencyInjection.


## 8) Serialization Extensions

- JSON (Newtonsoft.Json), XML, and CSV serializers are separate packages within src/RestSharp.Serializers.*.
- Keep serializer-specific behavior isolated. Avoid coupling core library to serializers.
- Each serializer project has its own tests; ensure behavior parity across TFMs.


## 9) Performance and Benchmarks

- Use benchmarks/RestSharp.Benchmarks for perf investigations.
- Before merging performance-related changes:
  - Validate allocations and throughput with BenchmarkDotNet where practical.
  - Check for regression across TFMs if the code path is shared.


## 10) Versioning and Packaging

- Versioning is handled by MinVer via Git tags and history. Ensure CI uses full history (unshallow fetch).
- Do not hardcode versions; rely on MinVer for assembly and file versions (CustomVersion target aligns versions post-MinVer).
- Packaging notes:
  - dotnet pack src/RestSharp/RestSharp.csproj -c Release -o nuget -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
  - Symbol packages must be .snupkg; SourceLink is enabled for debugging.
  - NuGet metadata is defined in src/Directory.Build.props; keep it accurate.


## 11) Continuous Integration

- PR workflow executes matrix tests on Windows (net48, net8.0, net9.0, net10.0) and Linux (net8.0, net9.0, net10.0).
- Build/Deploy workflow packages and pushes to NuGet on dev branch and tags using OIDC-based auth.
- To simulate CI locally:
  - dotnet test -c Debug -f net8.0
  - Optionally also run -f net9.0 and -f net10.0. On Windows, include net48.
- Uploading artifacts and publishing test results are CI-responsibilities; keep local output organized in test-results for quick inspection.


## 12) Code Organization and Style

- File organization:
  - Use partial classes to split large types by responsibility (e.g., RestClient.*, PropertyCache.*). Link related files via <DependentUpon> in csproj when appropriate.
- License header:
  - All source files under /src must include the standard repository license header.
  - Test files do not require the header.
- EditorConfig and rules:
  - Follow .editorconfig for formatting and analyzer rules.
  - In /src, suppress XML doc warnings via NoWarn=1591; in tests suppress xUnit1033 and CS8002 as configured.
- Nullable:
  - Treat nullable annotations and warnings as design signals; prefer explicit nullability and defensive checks at public boundaries.
- Public API surface:
  - Avoid breaking changes. If unavoidable, document in docs and changelog, and add clear migration notes.


## 13) PR Readiness Checklist

Use this quick checklist before requesting review:
- Builds cleanly across all targeted TFMs for affected projects.
- Unit/integration tests added or updated; all pass locally for relevant TFMs.
- No analyzer warnings introduced in src (beyond allowed suppressions).
- License header present in new /src files.
- Source generator changes validated (if applicable) by inspecting obj/.../generated output.
- Public API changes reviewed, documented, and tested.
- Central package versions unchanged unless intentionally updated with justification.
- Commit messages and PR description explain the why and the how.


## 14) Troubleshooting Guide

- Tests fail only on a specific TFM
  - Run with -f <TFM> to isolate. Look for conditional compilation and API availability differences.
- Generator output missing
  - Ensure EmitCompilerGeneratedFiles is true in the project under test; clean and rebuild; inspect obj/.../generated.
- net48 failures on non-Windows
  - Expected. Use net8.0 or higher on Linux/macOS. Run net48 only on Windows.
- MinVer version incorrect
  - Ensure CI or local clone has full git history (git fetch --prune --unshallow).
- Platform-specific failures
  - Use [UnsupportedOSPlatform] and conditional compilation to guard APIs not available on Browser, etc.


## 15) Useful Commands (Quick Reference)

- Build solution (Release):
  - dotnet build RestSharp.sln -c Release
- Run tests for a single TFM:
  - dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0
- Run a single test by fully-qualified name:
  - dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "FullyQualifiedName=RestSharp.Tests.ObjectParserTests.ShouldUseRequestProperty" -f net8.0
- Pack locally with symbols:
  - dotnet pack src/RestSharp/RestSharp.csproj -c Release -o nuget -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
- View generated source files after build:
  - find src/RestSharp/obj/Debug -name "*.g.cs" -o -name "ReadOnly*.cs"
- Clean all build artifacts:
  - dotnet clean RestSharp.sln
  - rm -rf src/*/bin src/*/obj test/*/bin test/*/obj gen/*/bin gen/*/obj


## 16) Documentation and References

- Main docs: https://restsharp.dev
- Repository: https://github.com/restsharp/RestSharp
- NuGet Packages:
  - RestSharp
  - RestSharp.Serializers.NewtonsoftJson
  - RestSharp.Serializers.Xml
  - RestSharp.Serializers.CsvHelper
- License: Apache-2.0

Keep this document up-to-date when build properties, TFMs, CI workflows, or repository conventions change.