# CNC CLI

Lightweight command-line adapter for CNC fabrication workflows with modular architecture.

## Architecture

- `Core/`: calibration, layout, sequencing, validation, time estimation, program transforms
- `MachineProfiles/`: pluggable machine profiles (`generic`, `hundegger`)
- `Adapters/`: export adapters (`plain`, `json`)
- `Observability/`: structured telemetry with Sentry-compatible sink

## Quick start

```bash
dotnet run --project /home/runner/work/tu-tools/tu-tools/cli/cnc-cli.csproj -- \
  --points "0,0,0;200,0,-3;200,80,-3" \
  --layout grid --rows 2 --cols 2 --spacingX 250 --spacingY 120 \
  --toolNumber 3 --diameter 6 --rpm 16000 --feed 2800 --plunge 700 --material "Birch Ply" --coolant true \
  --cutFeed 2800 --rapidFeed 8000 --accel 1800 --mass 90 --inertia 0.025 --rapidThreshold 35 \
  --profile hundegger --exportAdapter json \
  --template "G1 X{X} Y{Y} Z{Z} F{Feed}" --pair "X=120" --pair "Y=45" --pair "Z=-2" --pair "Feed=2400"
```

## Key flags

- Transform/layout: `--offsetX --offsetY --offsetZ --rotation --scale --layout`
- Capability packs: `--sequence --maxRapidStep --minZ`
- Machine profile: `--profile generic|hundegger`
- Export adapter: `--exportAdapter plain|json`
- Observability: `--environment --release --sampleRate --sentryDsn`
- Performance baseline: `--benchmark true --perfIterations 8000`

## Performance output

When benchmark mode is enabled:

- `BenchmarkIterations: <n>`
- `BenchmarkSeconds: <value>`

Used by CI as a regression gate.
