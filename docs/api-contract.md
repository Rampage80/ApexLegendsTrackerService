# Web API Contract

This is the backend repository's concise reference for AI-assisted API changes. The controller implementation and shared package remain authoritative.

- Route: `GET /api/v1/players/{platform}/{playerName}`
- Valid platforms: `PC`, `PS4`, `X1` (case-insensitive)
- The Web client URL-encodes both route values.
- Success responses are `ApexLegendsTracker.Shared.PlayerLookupResult`.
- The current shared package version consumed by the Tracker project is `1.5.0`.
- `IPlayerLookupContract` (the lookup query signature) is defined locally in this repo (`ApexLegendsTracker.GameStats.Services`), not in the shared package.
- The result uses the structured `Global`, `Realtime`, and `Legends` fields; do not reintroduce `RawJson` or fabricate statistics.
- `400` is returned for an empty player name or unsupported platform.
- Upstream failures are surfaced with the upstream status code and an `apex_upstream_error` response containing a trace identifier.
- Contract changes require coordinated updates to the backend, shared package, Web client, configuration, and relevant tests.
- The API base URL and API key are configured through `ApexApi`; CORS origins are configured through `Cors:AllowedOrigins`.
- Map rotation is exposed as `GET /api/v1/map-rotation`, with optional `version=1|2`.
- Predator thresholds are exposed as `GET /api/v1/predator-thresholds`.
- Map rotation responses use `MapRotationResponse`, with `BattleRoyale`, `Ranked`, `Ltm`, and `Wildcard` modes. Each mode has `Current` and `Next` entries sharing the `MapRotationEntry` shape. LTM entries include `EventName` when provided.
- Predator responses use `PredatorResponse.RP` with `PC`, `PS4`, and `X1` thresholds. The upstream `SWITCH` platform is intentionally excluded.
- The DTOs map the captured upstream response fields; do not invent or rename fields without a new contract decision.
- Invalid map-rotation versions return `400`; upstream failures and invalid upstream JSON return an error response with a trace identifier.
- Map rotation and Predator responses use a one-minute in-process memory cache to reduce upstream calls. Player lookups are intentionally not cached because each result is enriched with the requested player metadata.
- The current cache is local to one service process and is not shared across instances. Moving it to a distributed cache is a future improvement when horizontal scale or cross-instance cache consistency requires it.
- Chat is exposed as `POST /api/v1/chat`, accepting `ApexLegendsTracker.Shared.ChatRequest` (`Message` only) and returning `ChatResponse` (`Reply`, `Source`). `Source` is currently always `Knowledge`; a `Player` source for the existing player-lookup path is deferred future work, not yet implemented.
- Chat questions are answered directly by the configured OpenAI-compatible language model. The model settings are configured via `ApexAIChat:BaseUrl`/`Model`/`MaxOutputTokens`/`ApiKey`; questions are not restricted to a local knowledge pack or deterministic topic list.
- The deployed Service uses the generic OpenAI .NET SDK with the Azure OpenAI-compatible `/openai/v1/` endpoint. `ApexAIChat:ApiKey` is populated from the `APEXSERVICE_OPENAIKEY` environment variable (read explicitly in `Program.cs`, mirroring the existing `APEXSERVICE_APPINSIGHTS_CONNECTION_STRING` pattern) rather than through the standard `ApexAIChat__ApiKey` configuration binding.
- Chat answers use a dedicated in-process cache (`ICacheProvider`/`MemoryCacheProvider`, 24-hour duration, keyed by normalized question hash) that is independent of the Redis distributed cache used for map rotation/Predator. `ICacheProvider` is a reusable seam so this cache can be swapped to a distributed backend later.
- `/api/v1/chat` is rate-limited (10 requests/minute per instance via ASP.NET Core's built-in fixed-window limiter) and returns `400` for an empty or over-500-character message, `429` when rate-limited, and a `chat_upstream_error` response with a trace identifier for upstream model failures.
- The `ApexAIChat:ApiKey` must never be committed; configure it via user secrets locally and App Service configuration when deployed.
- Never commit or document API keys, tokens, or other credentials.
