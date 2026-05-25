import type { CutPhysicsOptions, CutTimeEstimate, Point } from "./types";

export function estimateTime(path: Point[], options: CutPhysicsOptions): CutTimeEstimate {
  if (path.length < 2) {
    return {
      cuttingSeconds: 0,
      rapidSeconds: 0,
      spinupSeconds: Math.max(0, options.spinupSeconds ?? 3),
      distance: 0,
      totalSeconds: Math.max(0, options.spinupSeconds ?? 3),
    };
  }

  const accel = Math.max(1e-6, options.acceleration / Math.max(0.5, 1 + options.mass * options.inertia));
  let cutting = 0;
  let rapids = 0;
  let distance = 0;

  for (let i = 1; i < path.length; i++) {
    const a = path[i - 1];
    const b = path[i];
    const d = distanceTo(a, b);
    distance += d;

    const isRapid = d >= options.rapidThreshold || a.z < b.z;
    const feed = Math.max(1e-6, (isRapid ? options.rapidFeed : options.cutFeed) / 60);

    const accelTime = feed / accel;
    const accelDist = 0.5 * accel * accelTime * accelTime;
    const segment = 2 * accelDist >= d ? 2 * Math.sqrt(d / accel) : 2 * accelTime + (d - 2 * accelDist) / feed;

    if (isRapid) rapids += segment;
    else cutting += segment;
  }

  const spin = Math.max(0, options.spinupSeconds ?? 3);
  return {
    cuttingSeconds: cutting,
    rapidSeconds: rapids,
    spinupSeconds: spin,
    distance,
    totalSeconds: cutting + rapids + spin,
  };
}

export function summarizeTime(est: CutTimeEstimate): string[] {
  return [
    `Distance: ${est.distance.toFixed(2)} mm`,
    `Cutting: ${est.cuttingSeconds.toFixed(2)} s`,
    `Rapids: ${est.rapidSeconds.toFixed(2)} s`,
    `Spin-up: ${est.spinupSeconds.toFixed(2)} s`,
    `Total: ${est.totalSeconds.toFixed(2)} s`,
  ];
}

export function runPerformanceBaseline(path: Point[], options: CutPhysicsOptions, iterations = 5000): number {
  const loops = Math.max(1, iterations);
  const start = Date.now();
  let total = 0;
  for (let i = 0; i < loops; i++) {
    total += estimateTime(path, options).totalSeconds;
  }
  if (total < 0) {
    throw new Error("Unreachable");
  }
  return (Date.now() - start) / 1000;
}

function distanceTo(a: Point, b: Point): number {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  const dz = a.z - b.z;
  return Math.sqrt(dx * dx + dy * dy + dz * dz);
}
