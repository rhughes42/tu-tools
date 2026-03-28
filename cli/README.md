# CNC CLI

Lightweight command-line helper for CNC fabrication workflows. It mirrors the Grasshopper components by applying calibration offsets, creating layouts, estimating cut time with simple physics, and emitting tool configuration headers.

## Quick start

```bash
dotnet run --project cli \
  -- --points "0,0,0;200,0,-3;200,80,-3" \
  --offsetX 10 --offsetY 5 --rotation 5 \
  --layout grid --rows 2 --cols 2 --spacingX 250 --spacingY 120 \
  --toolNumber 3 --diameter 6 --rpm 16000 --feed 2800 --plunge 700 --material "Birch Ply" --coolant true \
  --cutFeed 2800 --rapidFeed 8000 --accel 1800 --mass 90 --inertia 0.025 --rapidThreshold 35 \
  --template "G1 X{X} Y{Y} Z{Z} F{Feed}" --pair "X=120" --pair "Y=45" --pair "Z=-2" --pair "Feed=2400"
```

## Flags

- `--points` `x,y,z;...` base toolpath.
- Calibration: `--offsetX`, `--offsetY`, `--offsetZ`, `--rotation`, `--scale`.
- Layout: `--layout grid|mirrorX|mirrorY|rotate90|rotate180|rotate270`, `--rows`, `--cols`, `--spacingX`, `--spacingY`.
- Tool config: `--toolNumber`, `--diameter`, `--length`, `--rpm`, `--feed`, `--plunge`, `--material`, `--coolant true|false`.
- Physics/time: `--cutFeed`, `--rapidFeed`, `--accel`, `--mass`, `--inertia`, `--rapidThreshold`, `--spinup`.
- Custom commands: `--template` plus repeated `--pair key=value`.

Outputs include a human-readable tool summary, recommended header lines, expanded custom command, and a physics-based time estimate (acceleration-limited trapezoidal profile).
