# TypeScript CNC Toolkit

TypeScript adapter for modular CNC workflows.

## Modules

- `core/`: calibration, layout, sequencing, validation, time estimation, program transforms
- `machineProfiles/`: pluggable profile model + `hundegger` profile
- `adapters/`: export adapters (`plain`, `json`)
- `observability/`: structured telemetry + Sentry-compatible sink

## Usage

```ts
import {
  applyCalibration,
  layoutPoints,
  sequenceNearestNeighbor,
  validateToolpath,
  estimateTime,
  toolHeader,
  addLineNumbers,
  resolveMachineProfile,
  resolveExportAdapter,
  TelemetryClient,
  ConsoleTelemetrySink,
} from "tu-tools-cnc-ts";
```

## Performance baseline helper

Use `runPerformanceBaseline(path, options, iterations)` to produce a simple runtime baseline for regressions.
