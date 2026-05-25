import type { Point } from "./types";

export function applyCalibration(
  pts: Point[],
  offset: { x?: number; y?: number; z?: number; rotationDeg?: number; scale?: number }
): Point[] {
  const ox = offset.x ?? 0;
  const oy = offset.y ?? 0;
  const oz = offset.z ?? 0;
  const rotation = ((offset.rotationDeg ?? 0) * Math.PI) / 180;
  const scale = offset.scale ?? 1;
  const cos = Math.cos(rotation) * scale;
  const sin = Math.sin(rotation) * scale;

  return pts.map((p) => ({
    x: p.x * cos - p.y * sin + ox,
    y: p.x * sin + p.y * cos + oy,
    z: p.z * scale + oz,
  }));
}
