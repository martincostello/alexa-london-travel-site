# Coding Agent Instructions

This file provides guidance to agents when working with code in this repository.

## Build, test, and lint commands

- Use `.\build.ps1` from the repository root for the full CI-style path. It bootstraps the exact .NET SDK from `global.json`, publishes `src\LondonTravel.Site`, and runs the test suite.
- Use `.\build.ps1 -SkipTests` when you only need the publish/build step.
- Use `dotnet test .\tests\LondonTravel.Site.Tests\LondonTravel.Site.Tests.csproj -p:CollectCoverage=false` for normal local test runs. The test project enables coverlet thresholds by default, so focused runs should disable coverage.
- Run a single .NET test with `dotnet test .\tests\LondonTravel.Site.Tests\LondonTravel.Site.Tests.csproj --filter "FullyQualifiedName~MartinCostello.LondonTravel.Site.Integration.ApiTests.Schema_Is_Correct" -p:CollectCoverage=false`.
- Test categories are exposed as xUnit traits through `CategoryAttribute`; useful filters include `--filter "Category=Integration"` and `--filter "Category=EndToEnd"`.
- Frontend tooling lives in `src\LondonTravel.Site`. Run `npm run lint` for ESLint and `npm run test` for Vitest.
- Run a single frontend test file with `npm run test -- assets/scripts/Tracking.test.ts`.
- Use `npm run format-check` to verify frontend formatting without rewriting files. `npm run build` runs compile + format + lint, and the format step uses `--write`/`--fix`.

## High-level architecture

- `src\LondonTravel.Site` is the main ASP.NET Core application. `Program.cs` is intentionally thin; most service registration and middleware/endpoint wiring lives in `LondonTravelSiteBuilder.AddLondonTravelSite()` and `UseLondonTravelSite()`.
- `src\LondonTravel.Site.AppHost` is the .NET Aspire orchestrator for local/dev composition. It wires Azure Storage and Cosmos DB emulators plus the site project, so changes that depend on Azure-backed infrastructure often touch both the site and the app host.
- Request handling is split across several styles:
  - MVC controllers and Razor Pages for the web UI.
  - Minimal API modules such as `ApiModule`, `AlexaModule`, and `RedirectsModule`, which are mapped from `UseLondonTravelSite()`.
  - OpenAPI generation is enabled for the API surface and is validated by integration tests.
- Authentication is custom ASP.NET Core Identity over a Cosmos-backed document store. `AddApplicationAuthentication()` wires Identity, external OAuth providers, and the `admin` policy; `DocumentService` and `UserStore` back user persistence.
- Configuration is centered on `SiteOptions` (`Site:*` in `appsettings.json`). External auth providers, TfL settings, CSP/reporting, crawler redirects, and user-store settings are all bound from that options tree.
- TfL and Alexa behavior are service-driven. The UI and account flows depend on `ITflServiceFactory`, `IAccountService`, `AlexaService`, and the custom identity types rather than calling storage or external APIs directly.
- Tests are centralized in `tests\LondonTravel.Site.Tests`:
  - Integration tests use `WebApplicationFactory<Program>` via `TestServerFixture`.
  - The fixture swaps Cosmos-backed persistence for `InMemoryDocumentStore`.
  - External HTTP dependencies are replayed from interception bundles under `tests\LondonTravel.Site.Tests\Integration`.
  - Browser/end-to-end tests use Playwright fixtures and save screenshots, traces, and videos under `artifacts\...` when enabled.

## Key repository conventions

- Keep startup changes in the builder extensions, not in `Program.cs`. New services usually belong in `LondonTravelSiteBuilder`, `Extensions\*ServiceCollectionExtensions.cs`, or dedicated endpoint modules.
- Minimal APIs follow the module pattern: add a `MapXyz()` extension and call it from `UseLondonTravelSite()` instead of inlining endpoints in startup.
- JSON responses use the source-generated `ApplicationJsonSerializerContext` rather than ad-hoc serializer options. If you add or change API payloads, update the source-generation context and expect OpenAPI/snapshot tests to be affected.
- Logging prefers `[LoggerMessage]` partial methods for structured logs instead of inline message templates.
- The site is strict about URL shape: routing is lowercase and appends trailing slashes. Account for that in new routes and assertions.
- The frontend asset pipeline is part of the web project. `LondonTravel.Site.csproj` can invoke `npm run build` through the `BundleAssets` target, so frontend changes may affect .NET builds and publishes.
- Frontend formatting is an active mutation step. Avoid using `npm run build` just to check compilation if you do not want Prettier/stylelint to rewrite files.
- Integration and end-to-end tests are trait-based (`Integration`, `UI`, `EndToEnd`) and rely on shared fixtures. Reuse the existing fixtures/interception bundles instead of creating bespoke test hosts.
- Snapshot-style verification already exists (`Verify` plus `*.verified.*` files). When API or generated output changes intentionally, update the corresponding verified artifacts rather than replacing assertions with string comparisons.

## General guidelines

- Always ensure code compiles with no warnings or errors and tests pass locally before pushing changes.
- Do not use APIs marked with `[Obsolete]`.
- Bug fixes should **always** include a test that would fail without the corresponding fix.
- Do not introduce new dependencies unless specifically requested.
- Do not update existing dependencies unless specifically requested.
