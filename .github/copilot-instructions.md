# ApexLegendsTracker Service

- This repository is the ASP.NET Core backend and Azure Functions integration. Keep changes focused on the requested behavior.
- Preserve existing public APIs and the shared `ApexLegendsTracker.Shared` contract unless a contract change is explicitly requested.
- Inspect the nearest implementation, caller, configuration, and test before editing.
- Prefer the smallest change that follows an existing project pattern.
- Validate with the narrowest relevant test or `dotnet build`; report validation and unresolved risks briefly.
- Do not modify generated output under `bin/` or `obj/`.
- For endpoint, DTO, URL, serialization, CORS, authentication, or upstream API work, read `docs/api-contract.md` and verify the Web repository before changing the contract.
- Keep secrets out of source, logs, and shared coordination notes.
