---
applyTo: "**/*.cs"
---

# C# Guidance

- Follow existing dependency injection, async, nullability, and exception-handling patterns.
- Keep upstream HTTP behavior in services and keep endpoint validation and HTTP responses in controllers or functions.
- Use cancellation tokens when the surrounding API already supports them.
- Add or update focused tests for changed parsing, request construction, endpoint behavior, or configuration.
