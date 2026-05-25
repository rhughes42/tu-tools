import type { LayoutStrategy, Point } from "./types";

export function layoutPoints(
  pts: Point[],
  strategy: LayoutStrategy,
  options: { rows?: number; cols?: number; spacingX?: number; spacingY?: number } = {}
): Point[] {
  const rows = Math.max(1, options.rows ?? 1);
  const cols = Math.max(1, options.cols ?? 1);
  const sx = options.spacingX ?? 0;
  const sy = options.spacingY ?? 0;

  switch (strategy) {
    case "grid":
      return grid(pts, rows, cols, sx, sy);
    case "mirrorX":
      return mirror(pts, true);
    case "mirrorY":
      return mirror(pts, false);
    case "rotate90":
      return rotate(pts, 90);
    case "rotate180":
      return rotate(pts, 180);
    case "rotate270":
      return rotate(pts, 270);
    case "none":
    default:
      return pts;
  }
}

function grid(pts: Point[], rows: number, cols: number, sx: number, sy: number): Point[] {
  const result: Point[] = [];
  for (let r = 0; r < rows; r++) {
    for (let c = 0; c < cols; c++) {
      const dx = c * sx;
      const dy = r * sy;
      pts.forEach((p) => result.push({ x: p.x + dx, y: p.y + dy, z: p.z }));
    }
  }
  return result;
}

function mirror(pts: Point[], mirrorX: boolean): Point[] {
  return [
    ...pts,
    ...pts.map((p) => ({
      x: mirrorX ? -p.x : p.x,
      y: mirrorX ? p.y : -p.y,
      z: p.z,
    })),
  ];
}

function rotate(pts: Point[], deg: number): Point[] {
  const rad = (deg * Math.PI) / 180;
  const cos = Math.cos(rad);
  const sin = Math.sin(rad);
  return pts.map((p) => ({
    x: p.x * cos - p.y * sin,
    y: p.x * sin + p.y * cos,
    z: p.z,
  }));
}
