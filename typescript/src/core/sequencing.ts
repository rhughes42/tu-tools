import type { Point } from "./types";

export function sequenceNearestNeighbor(points: Point[]): Point[] {
  if (points.length <= 2) return points;

  const remaining = [...points];
  const ordered: Point[] = [remaining.shift() as Point];

  while (remaining.length) {
    const last = ordered[ordered.length - 1];
    let nearestIndex = 0;
    let nearestDistance = Number.POSITIVE_INFINITY;

    for (let i = 0; i < remaining.length; i++) {
      const d = distanceTo(last, remaining[i]);
      if (d < nearestDistance) {
        nearestDistance = d;
        nearestIndex = i;
      }
    }

    ordered.push(remaining.splice(nearestIndex, 1)[0]);
  }

  return ordered;
}

function distanceTo(a: Point, b: Point): number {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  const dz = a.z - b.z;
  return Math.sqrt(dx * dx + dy * dy + dz * dz);
}
