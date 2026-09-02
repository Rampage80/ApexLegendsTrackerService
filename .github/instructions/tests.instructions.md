---
applyTo: "**/*Tests/**/*.cs"
---

# Test Guidance

- Keep tests deterministic and focused on one behavior.
- Reuse the existing xUnit conventions and test project structure.
- Assert the public response, contract, or service behavior, not private implementation details.
- Run the focused test first, then widen to the project build when the change warrants it.
