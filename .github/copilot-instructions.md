# ApexLegendsTracker Service

- This repository is the ASP.NET Core backend and Azure Functions integration. Keep changes focused on the requested behavior.
- Preserve existing public APIs and the shared `ApexLegendsTracker.Shared` contract unless a contract change is explicitly requested.
- Inspect the nearest implementation, caller, configuration, and test before editing.
- Prefer the smallest change that follows an existing project pattern.
- Validate with the narrowest relevant test or `dotnet build`; report validation and unresolved risks briefly.
- Every production-code change must add or update an automated test. Use unit tests for backend logic and API behavior tests for endpoints; target at least 80% coverage of changed code and report the measured result.
- Generated runtime code must include structured logging at the appropriate `Trace`, `Debug`, `Information`, `Warning`, and `Error` levels, with no secrets or sensitive payloads in logs.
- Telemetry is required for every runtime feature and endpoint. Instrument request traces, meaningful metrics, and failures using the repository's Application Insights/OpenTelemetry conventions; do not generate unobservable code.
- Do not modify generated output under `bin/` or `obj/`.
- For endpoint, DTO, URL, serialization, CORS, authentication, or upstream API work, read `docs/api-contract.md` and verify the Web repository before changing the contract.
- Keep secrets out of source, logs, and shared coordination notes.
