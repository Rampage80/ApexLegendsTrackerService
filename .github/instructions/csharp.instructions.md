---
applyTo: "**/*.cs"
---

# C# Guidance

- Follow existing dependency injection, async, nullability, and exception-handling patterns.
- Keep upstream HTTP behavior in services and keep endpoint validation and HTTP responses in controllers or functions.
- Use cancellation tokens when the surrounding API already supports them.
- Add or update focused unit/API tests for changed parsing, request construction, endpoint behavior, or configuration; target at least 80% coverage of changed code.
- Generate structured logs at the appropriate `Trace`, `Debug`, `Information`, `Warning`, and `Error` levels, and instrument runtime paths with telemetry. Never log credentials or sensitive payloads.
