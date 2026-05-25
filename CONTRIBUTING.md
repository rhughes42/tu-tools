# Contributing

## Development standards

- Keep core logic framework-agnostic.
- Add/extend machine behavior through profile modules.
- Validate all external inputs at adapter boundaries.
- Keep telemetry PII-safe and structured.

## Local verification

- TypeScript: `cd /home/runner/work/tu-tools/tu-tools/typescript && npm test`
- CLI: `dotnet build /home/runner/work/tu-tools/tu-tools/cli/cnc-cli.csproj`

## Pull requests

- Use the PR checklist.
- Document architecture and behavior changes.
- Include performance notes for algorithmic changes.
