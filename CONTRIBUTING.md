# Contributing

## Development standards

- Keep core logic framework-agnostic.
- Add/extend machine behavior through profile modules.
- Validate all external inputs at adapter boundaries.
- Keep telemetry PII-safe and structured.

## Local verification

- TypeScript: `cd /home/runner/work/axis-cnc/axis-cnc/typescript && npm test`
- CLI: `dotnet build /home/runner/work/axis-cnc/axis-cnc/cli/cnc-cli.csproj`

## Pull requests

- Use the PR checklist.
- Document architecture and behavior changes.
- Include performance notes for algorithmic changes.
