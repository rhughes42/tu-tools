# Axis CNC

Fabrication tools for Technical University Dublin by Axis Consulting.

## Axis reintegration status

This repository now includes a modular migration architecture designed for reintegration into the Axis robotics framework:

- Core CNC logic and capability packs
- Adapter layers (CLI and TypeScript)
- Pluggable machine profiles (including initial Hundegger support)
- Sentry-compatible observability hooks
- CI performance baseline gate

See:
- `/home/runner/work/axis-cnc/axis-cnc/docs/axis-integration-scope.md`
- `/home/runner/work/axis-cnc/axis-cnc/docs/parity-baseline.md`
- `/home/runner/work/axis-cnc/axis-cnc/docs/architecture.md`

## Repository structure

- `/home/runner/work/axis-cnc/axis-cnc/AxisCNC`: Grasshopper plugin (.NET Framework)
- `/home/runner/work/axis-cnc/axis-cnc/cli`: modular .NET CLI adapter
- `/home/runner/work/axis-cnc/axis-cnc/typescript`: modular TypeScript adapter package
- `/home/runner/work/axis-cnc/axis-cnc/docs`: parity, architecture, config, runbook, release docs

## Capability packs implemented

- Program transforms (normalization + line numbering)
- Sequencing/scheduling (nearest-neighbor ordering)
- Richer validation (segment-size and Z-safe checks)
- Machine-safe template rendering
- Simulation/export adapters (plain + JSON simulation)

## Observability

- Structured telemetry context (`environment`, `release`, `module`)
- PII-safe redaction defaults
- Console sink + optional Sentry-compatible HTTP sink

## Specialist equipment

- Initial pluggable `hundegger` machine profile in CLI and TypeScript adapters
- Profile model designed for additional specialist machine modules

## CI

- TypeScript build/test job
- CLI build job
- CLI performance baseline gate (`--benchmark true`)

## Build notes

- TypeScript and CLI projects build in this environment.
- Legacy Grasshopper plugin targets .NET Framework 4.6.1 and requires a Windows/.NET Framework toolchain.
