# Architecture

## Module layers

1. **Core CNC domain**
   - Geometry primitives, calibration, layout, sequencing, validation, time estimation.
2. **Machine profiles**
   - Dialect rules and command transforms for specific machines (e.g., Hundegger).
3. **Adapter layer**
   - CLI and TypeScript wrappers over shared contracts.
4. **Observability layer**
   - Structured telemetry events and sinks (console + Sentry-compatible).

## Design principles

- Keep host/runtime concerns out of core modules.
- Favor pure functions and immutable outputs for deterministic CNC command generation.
- Make machine-specific behavior pluggable, not branch-specific.
