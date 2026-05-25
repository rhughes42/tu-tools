# Parity Baseline

This document defines the minimum parity bar required before Axis reintegration.

## Documentation parity

- Architecture overview and module boundaries.
- Module contracts and profile extension points.
- Configuration reference (runtime + observability).
- Runbook for incidents and troubleshooting.
- Contributing and release process guidance.

## Code standards parity

- Nullability enabled for modern .NET projects.
- Deterministic output formatting for CNC command generation.
- Shared naming/style conventions across adapters.
- Input validation at module boundaries.

## CI parity

- Build verification for TypeScript and CLI adapters.
- Performance baseline execution with regression gate.
- Security scan workflow support in hosted environment.

## Observability parity

- Structured events with module/environment/release tags.
- Optional Sentry-compatible sink with configurable sampling.
- PII-safe redaction defaults for telemetry payloads.

## Release parity

- Versioned release notes.
- SemVer-compatible package/release tagging.
- Checklist-based rollout milestones.
