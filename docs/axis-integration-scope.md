# Axis Integration Scope

This repository is being prepared for reintegration into the Axis robotics framework.

## Target repository and branch

- **Target repository**: `@Axis/<to-be-confirmed>`
- **Target branch**: `<to-be-confirmed>`
- **Integration owner**: Axis platform team

> Until these values are confirmed, this repository remains the source of truth for CNC module behavior and contracts.

## Reintegration boundaries

- Reusable CNC logic must live in framework-agnostic modules.
- Host-specific adapters (Grasshopper, CLI, TypeScript/Node) consume shared contracts.
- Machine-specific behavior is provided through pluggable profiles.

## Success criteria

- Functional parity with existing TU Tools capabilities.
- Observability parity (Sentry-compatible telemetry + contextual diagnostics).
- Documentation, CI, and release process parity with larger Axis repositories.
