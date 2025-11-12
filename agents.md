### RestSharp – Developer Notes (agents.md)

#### Scope
This document captures project-specific knowledge to speed up advanced development and maintenance of RestSharp. It focuses on build, configuration, and testing details unique to this repository, plus conventions and tips that help avoid common pitfalls.

---

### Build and configuration

- Solution layout
  - Root solution: `RestSharp.sln`.
  - Library sources in `src/RestSharp` targeting multiple frameworks via shared props.
  - Tests live under `test/` and are multi-targeted (see below).

- Multi-targeting
  - Tests target: `net48; net8.0; net9.0; net10.0` (defined in `test/Directory.Build.props`).
  - .NET Framework 4.8 support is provided via `Microsoft.NETFramework.ReferenceAssemblies.net472` for reference assemblies during build when `TargetFramework == net48`.
  - CI uses `actions/setup-dotnet@v4` with `dotnet-version: 9.0.x` for packaging; building tests locally may require multiple SDKs if you intend to run against all TFMs. Practically, you can run on a single installed TFM by overriding `-f`.
  - CI for pull requests runs tests against the supported .NET versions (.NET 8, .NET 9, and .NET 10) on Linux and Windows. On Windows, it also runs tests against .NET Framework 4.8.

- Central props
  - `test/Directory.Build.props` imports `props/Common.props` from the repo root. This propagates common settings into all test projects.
  - Notable properties:
    - `<IsTestProject>true</IsTestProject>` and `<IsPackable>false</IsPackable>` in tests.
    - `<Nullable>disable</Nullable>` in tests (be mindful when adding nullable-sensitive code in tests).
    - `<NoWarn>xUnit1033;CS8002</NoWarn>` to quiet specific analyzer warnings.
    - Test logs: `VSTestLogger` and `VSTestResultsDirectory` are preconfigured to write TRX per TFM into `test-results/<TFM>`.

- Packaging (FYI)
  - CI workflow `.github/workflows/build-dev.yml` demonstrates release packaging: `dotnet pack -c Release -o nuget -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg` and `dotnet nuget push` to nuget.org using OIDC via `NuGet/login@v1`.

---

### Testing

- Test frameworks and helpers
  - xUnit is the test framework. `test/Directory.Build.props` adds global `using` aliases for `Xunit`, `FluentAssertions`, and `AutoFixture` so you can use `[Fact]`, `Should()`, etc., without explicit `using` statements in each test file.
  - Additional packages commonly used in unit tests (see `test/RestSharp.Tests/RestSharp.Tests.csproj`): `Moq`, `RichardSzalay.MockHttp`, `System.Net.Http.Json`.
  - Integrated tests leverage `WireMockServer` (see `RestSharp.Tests.Integrated`) and use assets under `Assets/` where needed.

- Running tests
  - Run all tests for the entire solution:
    ```
    dotnet test RestSharp.sln -c Debug
    ```
  - Run a specific test project (multi-targeted will run for all installed TFMs):
    ```
    dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj
    ```
  - Select a single target framework (useful if you don’t have all SDKs installed):
    ```
    dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0
    ```
  - Run by fully-qualified name (FQN) — recommended for pinpointing a single test:
    ```
    dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj \
      --filter "FullyQualifiedName=RestSharp.Tests.UrlBuilderTests_Get.Should_build_url_with_query"
    ```
    Notes:
    - Prefer `FullyQualifiedName` for precision. Class and method names are case-sensitive.
    - You can combine with `-f net8.0` to avoid cross-TFM failures when only one SDK is present.
  - Run by namespace or class:
    ```
    dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "RestSharp.Tests.UrlBuilderTests"
    ```

- Logs and results
  - TRX logs are written per TFM into `test-results/<TFM>/` with file name `<ProjectName>.trx` as configured by `VSTestLogger`/`VSTestResultsDirectory` in `Directory.Build.props`.
  - To additionally emit console diagnostics:
    ```
    dotnet test -v n
    ```

- Code coverage
  - The `coverlet.collector` package is referenced for data-collector based coverage.
  - Example coverage run (generates cobertura xml):
    ```
    dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj \
      -f net8.0 \
      --collect:"XPlat Code Coverage" \
      -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Format=cobertura
    ```
  - Output will be placed under the test results directory for the given run.

- Adding new tests
  - New xUnit test files can be added anywhere under the corresponding test project directory; no extra `using` directives are required for `Xunit`/`FluentAssertions`/`AutoFixture` thanks to `Directory.Build.props` implicit usings.
  - Prefer co-locating tests by feature area and splitting large suites using partial classes (see `UrlBuilderTests.cs` with `UrlBuilderTests.Get.cs`/`Post.cs` linked via `DependentUpon` in the project) to keep navigation manageable.
  - For HTTP behavior, use `WireMockServer` in integrated tests rather than live endpoints. See `test/RestSharp.Tests.Integrated/DownloadFileTests.cs` for a pattern: spin up a server, register expectations in the constructor, and dispose in `IDisposable.Dispose`.
  - Follow existing assertions style with FluentAssertions.

- Verified example run
  - The test infrastructure was validated by executing a trivial `[Fact]` via fully qualified name using the built-in test runner. Use the FQN filtering example above to replicate.

---

### Additional development information

- Code style and analyzers
  - Adhere to the style used in `src/RestSharp` and existing tests. Test projects disable nullable by default; the main library might have different settings (check the respective `*.csproj` and imported props).
  - The repo uses central package management via `Directory.Packages.props`. Prefer bumping versions there unless a project has a specific override.

- HTTP/integration test guidance
  - Use `WireMockServer` for predictable, offline tests. Avoid time-sensitive or locale-sensitive assertions in integrated tests; when needed, pin formats (e.g., `"d"`) as seen in `ObjectParserTests`.
  - Be explicit about stream usage across TFMs. Some tests use `#if NET8_0_OR_GREATER` to select APIs like `Stream.ReadExactly`.

- Multi-TFM nuances
  - When debugging TFM-specific behavior, run `dotnet test -f <TFM>` to reproduce. Conditional compilation symbols (e.g., `NET8_0_OR_GREATER`) are used in tests; ensure your changes compile under all declared target frameworks or scope them with `#if`.

- Artifacts and outputs
  - NuGet packages are output to `nuget/` during local `dotnet pack` unless overridden.
  - Test artifacts are collected under `test-results/<TFM>/` per the configuration.

- Common pitfalls
  - Running tests targeting `net48` on non-Windows environments requires the reference assemblies (already pulled by package reference) but still may need Mono/compat setup on some systems; if unavailable, skip with `-f`.
  - Some integrated tests rely on asset files under `Assets/`. Ensure `AppDomain.CurrentDomain.BaseDirectory` resolves correctly when running from IDE vs CLI.

---

### Quick commands reference

```
# Build solution
dotnet build RestSharp.sln -c Release

# Run all tests for a single TFM
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj -f net8.0

# Run a single test by FQN
dotnet test test/RestSharp.Tests/RestSharp.Tests.csproj --filter "FullyQualifiedName=RestSharp.Tests.ObjectParserTests.ShouldUseRequestProperty"

# Pack (local)
dotnet pack src/RestSharp/RestSharp.csproj -c Release -o nuget -p:IncludeSymbols=true -p:SymbolPackageFormat=snupkg
```
