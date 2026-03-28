# TU Tools CNC TypeScript Package

TypeScript utilities that mirror the Grasshopper CNC helpers: layout planning, calibration offsets, tool configuration headers, custom command rendering, and physics-based cut time estimation.

## Install

```bash
cd typescript
npm install
npm run build
```

## Usage

```ts
import {
  applyCalibration,
  layoutPoints,
  renderCustomCommand,
  estimateTime,
  summarizeTime,
  toolHeader,
  Point,
} from "tu-tools-cnc-ts";

const path: Point[] = [
  { x: 0, y: 0, z: 0 },
  { x: 200, y: 0, z: -3 },
  { x: 200, y: 80, z: -3 },
];

const calibrated = applyCalibration(path, { offsetX: 5, offsetY: 10, rotationDeg: 3, scale: 1 });
const laidOut = layoutPoints(calibrated, "grid", { rows: 2, cols: 2, spacingX: 250, spacingY: 120 });

const time = estimateTime(laidOut, {
  cutFeed: 2800,
  rapidFeed: 8000,
  acceleration: 1800,
  mass: 90,
  inertia: 0.02,
  rapidThreshold: 35,
  spinupSeconds: 3,
});

console.log(summarizeTime(time));
console.log(toolHeader({
  toolNumber: 3,
  diameter: 6,
  length: 50,
  spindleRpm: 16000,
  feedRate: 2800,
  plungeRate: 700,
  material: "Birch Ply",
  coolant: true,
}));

console.log(renderCustomCommand("G1 X{X} Y{Y} Z{Z} F{F}", { X: 120, Y: 45, Z: -2, F: 2400 }));
```

The `estimateTime` helper uses a trapezoidal velocity profile with an acceleration reduced by mass and inertia to approximate real-world motion time for cutting versus rapid moves.
