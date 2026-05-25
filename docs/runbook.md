# CNC Runtime Runbook

## Failure triage

1. Confirm environment/release tags on telemetry events.
2. Identify failing module (`calibration`, `layout`, `validation`, `profile`, `export`).
3. Check validation output for machine safety violations.
4. Re-run benchmark command to detect performance regressions.

## Telemetry safety

- Do not emit raw user identifiers or secrets.
- Redact file-system paths and auth tokens.
- Use component/module context instead of free-form payload dumps.
