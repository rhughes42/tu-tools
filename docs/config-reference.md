# Configuration Reference

## Observability

- `TU_ENVIRONMENT`: logical environment tag (`dev`, `staging`, `prod`)
- `TU_RELEASE`: release/version tag attached to telemetry
- `TU_SENTRY_DSN`: optional DSN/endpoint for Sentry-compatible sink
- `TU_TELEMETRY_SAMPLE_RATE`: range `0.0` to `1.0`
- `TU_TELEMETRY_MAX_BREADCRUMBS`: optional breadcrumb cap

## Performance

- `TU_PERF_ITERATIONS`: benchmark iterations for local/CI checks
- `TU_PERF_MAX_SECONDS`: max allowed benchmark duration in CI

## CNC behavior

- `TU_RAPID_THRESHOLD_MM`: default rapid/cut classification threshold
- `TU_DEFAULT_SPINUP_SECONDS`: spindle overhead default
