---
applyTo: "**/*Tests/**/*.cs"
---

# Test Guidance

- Keep tests deterministic and focused on one behavior.
- Reuse the existing xUnit conventions and test project structure.
- Assert the public response, contract, or service behavior, not private implementation details.
- Every production-code change must have a corresponding automated test change. Prefer unit tests for isolated logic and API-level tests for endpoint behavior; target at least 80% coverage of changed code.
- Verify telemetry and structured logging behavior for important success, dependency, validation, and failure paths without asserting unstable logger formatting.
- Run the focused test first, then widen to the project build when the change warrants it.
