# Web API Contract

This is the backend repository's concise reference for AI-assisted API changes. The controller implementation and shared package remain authoritative.

- Route: `GET /api/v1/players/{platform}/{playerName}`
- Valid platforms: `PC`, `PS4`, `X1` (case-insensitive)
- The Web client URL-encodes both route values.
- Success responses are `ApexLegendsTracker.Shared.PlayerLookupResult`.
- The current shared package version consumed by the Service project is `1.1.0`.
- The result uses the structured `Global`, `Realtime`, and `Legends` fields; do not reintroduce `RawJson` or fabricate statistics.
- `400` is returned for an empty player name or unsupported platform.
- Upstream failures are surfaced with the upstream status code and an `apex_upstream_error` response containing a trace identifier.
- Contract changes require coordinated updates to the backend, shared package, Web client, configuration, and relevant tests.
- The API base URL and API key are configured through `ApexApi`; CORS origins are configured through `Cors:AllowedOrigins`.
- Never commit or document API keys, tokens, or other credentials.
