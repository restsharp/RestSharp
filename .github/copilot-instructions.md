# Copilot Instructions for RestSharp

This document provides instructions for GitHub Copilot when working with the RestSharp repository.

## Project Overview

RestSharp is a lightweight HTTP API client library for .NET. It wraps `HttpClient` and provides:
- Default parameters of any kind, not just headers
- Multiple ways to add request parameters (query, URL segment, header, cookie, body)
- Built-in serialization/deserialization for JSON, XML, and CSV
- Rich support for authentication

## Repository Structure

- `src/RestSharp/` - Main library
- `src/RestSharp.Serializers.*/` - Serializer extensions (NewtonsoftJson, Xml, CsvHelper)
- `gen/SourceGenerator/` - Incremental source generators
- `test/RestSharp.Tests/` - Unit tests
- `test/RestSharp.Tests.Integrated/` - Integration tests (WireMock)
- `test/RestSharp.Tests.Serializers.*/` - Serializer-specific tests
- `benchmarks/RestSharp.Benchmarks/` - Performance tests

## Build and Test Commands

```bash
# Build solution
dotnet build RestSharp.sln -c Debug

# Run all tests
dotnet test RestSharp.sln -c Debug

# Run tests for specific TFM
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0

# Run single test
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "FullyQualifiedName=Namespace.Class.Method" -f net8.0

# Pack for release
dotnet pack src/RestSharp/RestSharp.csproj -c Release -o nuget -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
```

## Multi-Targeting

The library targets multiple frameworks:
- `netstandard2.0`, `net471`, `net48` - Legacy support
- `net8.0`, `net9.0`, `net10.0` - Modern .NET

Tests target: `net48` (Windows only), `net8.0`, `net9.0`, `net10.0`

When adding features:
- Use conditional compilation for TFM-specific APIs: `#if NET` or `#if NET8_0_OR_GREATER`
- Ensure compilation succeeds for all TFMs
- Add polyfills or conditional code for missing APIs on older TFMs

## Code Style and Conventions

- Language version: C# preview (latest features allowed)
- Nullable reference types: Enabled in `/src`, disabled in `/test`
- All `/src` files must include the Apache 2.0 license header
- Test files do not require the license header
- Follow `.editorconfig` for formatting rules
- Use partial classes for large types (link with `<DependentUpon>`)
- Assemblies are strong-named via `RestSharp.snk`

### License Header for Source Files

```csharp
//  Copyright (c) .NET Foundation and Contributors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
```

## Testing Guidelines

- Frameworks: xUnit + FluentAssertions + AutoFixture
- Use WireMockServer for HTTP scenarios (avoid live endpoints)
- Use descriptive assertions: `result.Should().Be(expected)`
- Guard TFM-specific tests with `#if NET8_0_OR_GREATER`
- Avoid flaky tests: don't depend on timing, locale, or network conditions

Test projects have global usings configured for `Xunit`, `FluentAssertions`, and `AutoFixture`.

## Source Generators

Custom generators in `gen/SourceGenerator/`:
- `[GenerateImmutable]` - Creates read-only wrappers
- `[GenerateClone]` - Creates static factory clone methods
- `[Exclude]` - Excludes properties from immutable generation

Generator target: `netstandard2.0` (required for all compiler versions)

## Dependencies

Package versions are centrally managed in `Directory.Packages.props`. Do not pin versions in individual projects unless justified by TFM constraints.

## PR Checklist

Before submitting:
- Builds cleanly across all targeted TFMs
- Tests added/updated and passing
- No new analyzer warnings in `/src`
- License header in new `/src` files
- Public API changes documented and tested
