# Configuration Reference

## Observability

- `AXIS_ENVIRONMENT`: logical environment tag (`dev`, `staging`, `prod`)
- `AXIS_RELEASE`: release/version tag attached to telemetry
- `AXIS_SENTRY_DSN`: optional DSN/endpoint for Sentry-compatible sink
- `AXIS_TELEMETRY_SAMPLE_RATE`: range `0.0` to `1.0`
- `AXIS_TELEMETRY_MAX_BREADCRUMBS`: optional breadcrumb cap

## Performance

- `AXIS_PERF_ITERATIONS`: benchmark iterations for local/CI checks
- `AXIS_PERF_MAX_SECONDS`: max allowed benchmark duration in CI

## CNC behavior

- `AXIS_RAPID_THRESHOLD_MM`: default rapid/cut classification threshold
- `AXIS_DEFAULT_SPINUP_SECONDS`: spindle overhead default
