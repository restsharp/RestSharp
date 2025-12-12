### RestSharp – Developer Notes (agents.md)

#### Scope
This document captures project-specific knowledge to speed up advanced development and maintenance of RestSharp. It focuses on build, configuration, testing details, source generation, and conventions unique to this repository.

---

## Solution Structure

### Projects and Organization

The solution (`RestSharp.sln`) is organized into the following structure:

**Core Library:**
- `src/RestSharp/` - Main library targeting multiple frameworks

**Serializer Extensions:**
- `src/RestSharp.Serializers.NewtonsoftJson/` - Newtonsoft.Json serializer
- `src/RestSharp.Serializers.Xml/` - XML serializer
- `src/RestSharp.Serializers.CsvHelper/` - CSV serializer

**Source Generator:**
- `gen/SourceGenerator/` - Incremental source generator for code generation (see dedicated section below)

**Test Projects:**
- `test/RestSharp.Tests/` - Core unit tests
- `test/RestSharp.Tests.Integrated/` - Integration tests using WireMock
- `test/RestSharp.Tests.Serializers.Json/` - JSON serializer tests
- `test/RestSharp.Tests.Serializers.Xml/` - XML serializer tests
- `test/RestSharp.Tests.Serializers.Csv/` - CSV serializer tests
- `test/RestSharp.Tests.Shared/` - Shared test utilities
- `test/RestSharp.InteractiveTests/` - Interactive/manual tests

**Performance:**
- `benchmarks/RestSharp.Benchmarks/` - BenchmarkDotNet performance tests

---

## Build and Configuration

### Multi-Targeting

**Library Targets** (`src/Directory.Build.props`):
- `netstandard2.0` - .NET Standard 2.0 for broad compatibility
- `net471` - .NET Framework 4.7.1
- `net48` - .NET Framework 4.8
- `net8.0` - .NET 8
- `net9.0` - .NET 9
- `net10.0` - .NET 10

**Test Targets** (`test/Directory.Build.props`):
- `net48` - .NET Framework 4.8 (Windows only)
- `net8.0` - .NET 8
- `net9.0` - .NET 9
- `net10.0` - .NET 10

**Source Generator Target:**
- `netstandard2.0` - Required for source generators to work across all compiler versions

### Build Properties Hierarchy

The build system uses a hierarchical props structure:

1. **`props/Common.props`** - Root properties imported by all projects:
    - Sets `RepoRoot` variable
    - Configures assembly signing (`RestSharp.snk`)
    - Sets `LangVersion=preview` and `ImplicitUsings=enable`
    - Enables nullable reference types (`Nullable=enable`)
    - Adds global `using System.Net.Http;`

2. **`src/Directory.Build.props`** - Source project properties:
    - Imports `Common.props`
    - Defines multi-targeting for libraries
    - Configures NuGet package metadata (icon, license, description)
    - Enables SourceLink for debugging
    - Uses MinVer for versioning
    - Conditionally adds polyfills for older frameworks
    - Generates XML documentation files

3. **`test/Directory.Build.props`** - Test project properties:
    - Imports `Common.props`
    - Sets `IsTestProject=true` and `IsPackable=false`
    - Configures test result output: `test-results/<TFM>/<ProjectName>.trx`
    - Disables nullable (`Nullable=disable` for tests)
    - Adds global usings for xUnit, FluentAssertions, AutoFixture
    - Suppresses warnings: `xUnit1033`, `CS8002`

4. **`Directory.Packages.props`** - Central Package Management:
    - All package versions defined centrally
    - TFM-specific version overrides (e.g., `System.Text.Json` for .NET 10)
    - Separate sections for runtime, compile, and testing dependencies

### Framework-Specific Considerations

**Legacy Framework Support (.NET Framework 4.7.1/4.8, netstandard2.0):**
- `System.Text.Json` is added as a package reference (newer frameworks have it built-in)
- Polyfills are enabled via `Nullable` package for nullable reference type attributes
- Reference assemblies provided by `Microsoft.NETFramework.ReferenceAssemblies.net472`

**Modern .NET (8/9/10):**
- Native support for most features
- Conditional compilation using `#if NET`
- Platform-specific attributes like `[UnsupportedOSPlatform("browser")]`

### Assembly Signing

All assemblies are strong-named using `RestSharp.snk` (configured in `Common.props`).

---

## Source Generator

RestSharp includes a custom incremental source generator located in `gen/SourceGenerator/` that automates boilerplate code generation.

### Generator Architecture

**Project Configuration:**
- Targets: `netstandard2.0` (required for source generators)
- Language: C# preview features enabled
- Output: Not included in build output (`IncludeBuildOutput=false`)
- Analyzer rules: Extended analyzer rules enforced
- Referenced as analyzer in main project: `OutputItemType="Analyzer"`

**Dependencies:**
- `Microsoft.CodeAnalysis.Analyzers` - Analyzer SDK
- `Microsoft.CodeAnalysis.CSharp` - Roslyn C# APIs

**Global Usings:**
```csharp
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;
```

### Generator Components

#### 1. ImmutableGenerator (`ImmutableGenerator.cs`)

**Purpose:** Generates immutable (read-only) wrapper classes from mutable classes.

**Trigger Attribute:** `[GenerateImmutable]`

**How It Works:**
1. Scans compilation for classes annotated with `[GenerateImmutable]`
2. Identifies all properties with `set` accessors (excluding those marked with `[Exclude]`)
3. Generates a `ReadOnly{ClassName}` partial class with:
    - Read-only properties (getters only)
    - Constructor accepting the mutable class instance
    - Partial method `CopyAdditionalProperties` for extensibility
    - Preserves XML documentation comments

**Example Usage:**
```csharp
[GenerateImmutable]
public class RestClientOptions {
    public Uri? BaseUrl { get; set; }
    public string? UserAgent { get; set; }
    [Exclude]  // This property won't be in the immutable version
    public List<Interceptor> Interceptors { get; set; }
}
```

**Generated Output:** `ReadOnlyRestClientOptions.cs` with immutable properties and a constructor that copies values from `RestClientOptions`.

**Location:** Generated files appear in `obj/<Configuration>/<TFM>/generated/SourceGenerator/SourceGenerator.ImmutableGenerator/`

#### 2. InheritedCloneGenerator (`InheritedCloneGenerator.cs`)

**Purpose:** Generates static factory methods to clone objects from base types to derived types.

**Trigger Attribute:** `[GenerateClone(BaseType = typeof(BaseClass), Name = "MethodName")]`

**How It Works:**
1. Finds classes with `[GenerateClone]` attribute
2. Extracts `BaseType` and `Name` from attribute parameters
3. Analyzes properties from the base type and its inheritance chain
4. Generates a static factory method that:
    - Takes the base type as parameter
    - Creates a new instance of the derived type
    - Copies all properties from base to derived
    - Uses constructor parameters where applicable

**Example Usage:**
```csharp
[GenerateClone(BaseType = typeof(RestResponse), Name = "FromResponse")]
public partial class RestResponse<T> : RestResponse {
    public T? Data { get; set; }
}
```

**Generated Output:** `RestResponse.Clone.g.cs` with a static `FromResponse` method that creates `RestResponse<T>` from `RestResponse`.

**Location:** Generated files appear in `obj/<Configuration>/<TFM>/generated/SourceGenerator/SourceGenerator.InheritedCloneGenerator/`

#### 3. Extensions (`Extensions.cs`)

**Purpose:** Helper extension methods for the generators using C# extension types.

**Key Methods:**
- `FindClasses(predicate)` - Finds classes matching a predicate across all syntax trees
- `FindAnnotatedClasses(attributeName, strict)` - Finds classes with specific attributes
- `GetBaseTypesAndThis()` - Traverses type hierarchy to get all base types

### Attribute Definitions

Located in `src/RestSharp/Extensions/GenerateImmutableAttribute.cs`:

```csharp
[AttributeUsage(AttributeTargets.Class)]
class GenerateImmutableAttribute : Attribute;

[AttributeUsage(AttributeTargets.Class)]
class GenerateCloneAttribute : Attribute {
    public Type? BaseType { get; set; }
    public string? Name { get; set; }
}

[AttributeUsage(AttributeTargets.Property)]
class Exclude : Attribute;  // Excludes properties from immutable generation
```

### Integration with Main Project

In `src/RestSharp/RestSharp.csproj`:
```xml
<PropertyGroup>
    <EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>
</PropertyGroup>

<ItemGroup Label="Source generator">
    <ProjectReference Include="$(RepoRoot)\gen\SourceGenerator\SourceGenerator.csproj" 
                      OutputItemType="Analyzer" 
                      ReferenceOutputAssembly="false" />
</ItemGroup>
```

### Debugging Generated Code

Generated files are emitted to the `obj` directory when `EmitCompilerGeneratedFiles=true`. To view:
```bash
# Example path for net8.0 Debug build
ls src/RestSharp/obj/Debug/net8.0/generated/SourceGenerator/
```

---

## Testing

### Test Framework and Helpers

**Primary Framework:** xUnit

**Assertion Library:** FluentAssertions

**Test Data:** AutoFixture

**Mocking:**
- `Moq` - General mocking
- `RichardSzalay.MockHttp` - HTTP message handler mocking
- `WireMock.Net` - HTTP server mocking for integration tests

**Global Usings** (configured in `test/Directory.Build.props`):
```csharp
using Xunit;
using Xunit.Abstractions;
using FluentAssertions;
using FluentAssertions.Extensions;
using AutoFixture;
```

These are automatically available in all test files without explicit `using` statements.

### Test Project Organization

**Unit Tests (`RestSharp.Tests`):**
- Tests for core functionality
- Uses mocking for HTTP interactions
- Example: `UrlBuilderTests`, `ObjectParserTests`
- Organized with partial classes for large test suites (e.g., `UrlBuilderTests.Get.cs`, `UrlBuilderTests.Post.cs`)

**Integration Tests (`RestSharp.Tests.Integrated`):**
- Uses `WireMockServer` for realistic HTTP scenarios
- Tests actual HTTP behavior without external dependencies
- Example: `DownloadFileTests` spins up WireMock server in constructor, disposes in `IDisposable.Dispose`
- Asset files stored in `Assets/` directory

**Serializer Tests:**
- Separate projects for each serializer (JSON, XML, CSV)
- Test serialization/deserialization behavior

### Running Tests

**All tests for entire solution:**
```bash
dotnet test RestSharp.sln -c Debug
```

**Specific test project:**
```bash
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj
```

**Single target framework:**
```bash
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0
```

**Single test by fully-qualified name (recommended for precision):**
```bash
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj \
  --filter "FullyQualifiedName=RestSharp.Tests.UrlBuilderTests_Get.Should_build_url_with_query" \
  -f net8.0
```

**Filter by namespace or class:**
```bash
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj \
  --filter "RestSharp.Tests.UrlBuilderTests"
```

**With verbose output:**
```bash
dotnet test -v n
```

### Test Results and Logging

**Output Location:** `test-results/<TFM>/<ProjectName>.trx`

**Configuration** (in `test/Directory.Build.props`):
```xml
<VSTestLogger>trx%3bLogFileName=$(MSBuildProjectName).trx</VSTestLogger>
<VSTestResultsDirectory>$(RepoRoot)/test-results/$(TargetFramework)</VSTestResultsDirectory>
```

Results are written per target framework, making it easy to identify TFM-specific failures.

### Code Coverage

**Tool:** coverlet.collector (data-collector based)

**Generate coverage report:**
```bash
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj \
  -f net8.0 \
  --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
```

Coverage output is placed in the test results directory.

### Adding New Tests

**Best Practices:**
1. Co-locate tests by feature area
2. Use partial classes for large test suites (link via `<DependentUpon>` in `.csproj`)
3. For HTTP tests, prefer `WireMockServer` over live endpoints
4. Use FluentAssertions for readable assertions: `result.Should().Be(expected)`
5. Avoid time-sensitive or locale-sensitive assertions; pin formats when needed
6. Use `#if NET8_0_OR_GREATER` for TFM-specific APIs

**Example Test Structure:**
```csharp
public class MyFeatureTests {
    [Fact]
    public void Should_do_something() {
        // Arrange
        var fixture = new Fixture();
        var input = fixture.Create<string>();
        
        // Act
        var result = MyFeature.Process(input);
        
        // Assert
        result.Should().NotBeNull();
    }
}
```

---

## Continuous Integration

### CI Workflows

**Location:** `.github/workflows/`

#### 1. Pull Request Workflow (`pull-request.yml`)

**Triggers:** Pull requests (excluding `docs/**` changes)

**Test Matrix:**
- **Windows:** Tests against `net48`, `net8.0`, `net9.0`, `net10.0`
- **Linux:** Tests against `net8.0`, `net9.0`, `net10.0` (no .NET Framework)

**SDK Setup:**
```yaml
dotnet-version: |
  8.0.x
  9.0.x
  10.0.x
```

**Test Command:**
```bash
dotnet test -c Debug -f ${{ matrix.dotnet }}
```

**Artifacts:** Test results uploaded for each TFM and OS combination

#### 2. Build and Deploy Workflow (`build-dev.yml`)

**Triggers:**
- Push to `dev` branch
- Tags (for releases)

**SDK:** .NET 10.0.x (for packaging)

**Steps:**
1. Checkout with full history (`git fetch --prune --unshallow` for MinVer)
2. NuGet login using OIDC (`NuGet/login@v1`)
3. Pack: `dotnet pack -c Release -o nuget -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg`
4. Push to NuGet.org with `--skip-duplicate`

**Permissions:** Requires `id-token: write` for OIDC authentication

#### 3. Test Results Workflow (`test-results.yml`)

Publishes test results as GitHub checks.

### Local CI Simulation

To replicate CI behavior locally:

**Windows (all TFMs):**
```bash
dotnet test -c Debug -f net48
dotnet test -c Debug -f net8.0
dotnet test -c Debug -f net9.0
dotnet test -c Debug -f net10.0
```

**Linux/macOS (no .NET Framework):**
```bash
dotnet test -c Debug -f net8.0
dotnet test -c Debug -f net9.0
dotnet test -c Debug -f net10.0
```

---

## Versioning and Packaging

### Versioning Strategy

**Tool:** MinVer (Git-based semantic versioning)

**Configuration** (in `src/Directory.Build.props`):
```xml
<PackageReference Include="MinVer" PrivateAssets="All"/>
```

**Custom Version Target:**
```xml
<Target Name="CustomVersion" AfterTargets="MinVer">
    <PropertyGroup>
        <FileVersion>$(MinVerMajor).$(MinVerMinor).$(MinVerPatch)</FileVersion>
        <AssemblyVersion>$(MinVerMajor).$(MinVerMinor).$(MinVerPatch)</AssemblyVersion>
    </PropertyGroup>
</Target>
```

Version is determined from Git tags and commit history. Requires unshallow clone for accurate versioning.

### Package Configuration

**NuGet Metadata:**
- Icon: `restsharp.png`
- License: Apache-2.0
- Project URL: https://restsharp.dev
- Repository: https://github.com/restsharp/RestSharp.git
- README: Included in package

**Symbol Packages:** `.snupkg` format for debugging

**SourceLink:** Enabled for source debugging

### Local Packaging

```bash
dotnet pack src/RestSharp/RestSharp.csproj -c Release -o nuget \
  -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
```

Output: `nuget/RestSharp.<version>.nupkg` and `RestSharp.<version>.snupkg`

---

## Code Organization and Conventions

### File Organization

**Partial Classes:** Large classes are split using partial classes with `<DependentUpon>` in `.csproj`:
```xml
<Compile Update="RestClient.Async.cs">
    <DependentUpon>RestClient.cs</DependentUpon>
</Compile>
```

Examples:
- `RestClient.cs` with `RestClient.Async.cs`, `RestClient.Extensions.*.cs`
- `PropertyCache.cs` with `PropertyCache.Populator.cs`, `PropertyCache.Populator.RequestProperty.cs`

### Code Style

- `.editorconfig` is used for code formatting and style rules
- All source files in `/src` must have a license header:
    ```text
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
    //
    // Adapted from Rebus
    ```
- Test files (all projects located in `/test`) don't need the license header

**Nullable Reference Types:**
- Enabled in source projects (`Nullable=enable`)
- Disabled in test projects (`Nullable=disable`)

**Language Version:** `preview` - allows use of latest C# features

**Implicit Usings:** Enabled globally

**Warnings:**
- XML documentation warnings suppressed in source (`NoWarn=1591`)
- Test-specific warnings suppressed (`xUnit1033`, `CS8002`)

### Platform-Specific Code

Use conditional compilation and attributes:

```csharp
#if NET
[UnsupportedOSPlatform("browser")]
#endif
public ICredentials? Credentials { get; set; }
```

```csharp
#if NET8_0_OR_GREATER
await using var stream = ...
#else
using var stream = ...
#endif
```

---

## Common Development Tasks

### Building the Solution

**Debug build:**
```bash
dotnet build RestSharp.sln -c Debug
```

**Release build:**
```bash
dotnet build RestSharp.sln -c Release
```

### Working with Source Generator

**View generated files:**
```bash
# After building
find src/RestSharp/obj -name "*.g.cs" -o -name "ReadOnly*.cs"
```

**Debug generator:**
1. Set `<EmitCompilerGeneratedFiles>true</EmitCompilerGeneratedFiles>` in project
2. Build project
3. Check `obj/<Configuration>/<TFM>/generated/SourceGenerator/`

**Add new generator:**
1. Create new class implementing `IIncrementalGenerator`
2. Add `[Generator(LanguageNames.CSharp)]` attribute
3. Implement `Initialize` method
4. Register source output

### Multi-TFM Development

**Build for specific TFM:**
```bash
dotnet build src/RestSharp/RestSharp.csproj -f net8.0
```

**Check TFM-specific behavior:**
- Use `#if` directives for conditional compilation
- Test against all supported TFMs before committing
- Be aware of API differences (e.g., `Stream.ReadExactly` in .NET 8+)

### Troubleshooting

**Issue:** Tests fail on specific TFM
- **Solution:** Run with `-f <TFM>` to isolate, check for TFM-specific APIs

**Issue:** Source generator not running
- **Solution:** Clean and rebuild, check `EmitCompilerGeneratedFiles` setting

**Issue:** .NET Framework tests fail on non-Windows
- **Solution:** Expected behavior; run with `-f net8.0` or higher on Linux/macOS

**Issue:** MinVer version incorrect
- **Solution:** Ensure full Git history with `git fetch --prune --unshallow`

---

## Quick Reference Commands

```bash
# Build solution
dotnet build RestSharp.sln -c Release

# Run all tests for a single TFM
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0

# Run a single test by FQN
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj \
  --filter "FullyQualifiedName=RestSharp.Tests.ObjectParserTests.ShouldUseRequestProperty" \
  -f net8.0

# Pack locally
dotnet pack src/RestSharp/RestSharp.csproj -c Release -o nuget \
  -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg

# Generate code coverage
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0 \
  --collect:"XPlat Code Coverage" \
  -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura

# View generated source files
find src/RestSharp/obj/Debug -name "*.g.cs" -o -name "ReadOnly*.cs"

# Clean all build artifacts
dotnet clean RestSharp.sln
rm -rf src/*/bin src/*/obj test/*/bin test/*/obj gen/*/bin gen/*/obj
```

---

## Additional Resources

- **Main Documentation:** https://restsharp.dev
- **Repository:** https://github.com/restsharp/RestSharp
- **NuGet Package:** https://www.nuget.org/packages/RestSharp
- **License:** Apache-2.0
