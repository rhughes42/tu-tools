export type Point = { x: number; y: number; z: number };

export type ToolConfiguration = {
  toolNumber: number;
  diameter: number;
  length: number;
  spindleRpm: number;
  feedRate: number;
  plungeRate: number;
  material: string;
  coolant: boolean;
};

export type LayoutStrategy =
  | "grid"
  | "mirrorX"
  | "mirrorY"
  | "rotate90"
  | "rotate180"
  | "rotate270"
  | "none";

export type CutPhysicsOptions = {
  cutFeed: number; // mm/min
  rapidFeed: number; // mm/min
  acceleration: number; // mm/s^2
  mass: number; // kg
  inertia: number; // multiplier
  rapidThreshold: number; // mm
  spinupSeconds?: number;
};

export type CutTimeEstimate = {
  cuttingSeconds: number;
  rapidSeconds: number;
  spinupSeconds: number;
  distance: number;
  totalSeconds: number;
};

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

export function renderCustomCommand(template: string, parameters: Record<string, string | number>): string {
  return Object.keys(parameters).reduce((cmd, key) => {
    return cmd.replace(new RegExp(`\\{${key}\\}`, "g"), String(parameters[key]));
  }, template);
}

export function toolHeader(cfg: ToolConfiguration): string[] {
  const lines = [
    `( Tool ${cfg.toolNumber} | Dia ${cfg.diameter.toFixed(2)}mm | ${cfg.material} )`,
    `T${cfg.toolNumber} M06`,
    `S${cfg.spindleRpm.toFixed(0)} M03`,
    `F${cfg.feedRate.toFixed(1)}`,
    `( Plunge ${cfg.plungeRate.toFixed(1)} mm/min )`,
  ];

  if (cfg.coolant) lines.push("M08");
  return lines;
}

export function estimateTime(path: Point[], options: CutPhysicsOptions): CutTimeEstimate {
  if (path.length < 2) {
    return { cuttingSeconds: 0, rapidSeconds: 0, spinupSeconds: options.spinupSeconds ?? 3, distance: 0, totalSeconds: 0 };
  }

  const accel = options.acceleration / Math.max(0.5, 1 + options.mass * options.inertia);
  let cutting = 0;
  let rapids = 0;
  let distance = 0;

  for (let i = 1; i < path.length; i++) {
    const a = path[i - 1];
    const b = path[i];
    const d = distanceTo(a, b);
    distance += d;

    const isRapid = d >= options.rapidThreshold || a.z < b.z;
    const feed = (isRapid ? options.rapidFeed : options.cutFeed) / 60;

    const accelTime = feed / accel;
    const accelDist = 0.5 * accel * accelTime * accelTime;
    let segment: number;
    if (2 * accelDist >= d) {
      segment = 2 * Math.sqrt(d / accel);
    } else {
      const cruise = d - 2 * accelDist;
      segment = 2 * accelTime + cruise / feed;
    }

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

// Helpers
function distanceTo(a: Point, b: Point): number {
  const dx = a.x - b.x;
  const dy = a.y - b.y;
  const dz = a.z - b.z;
  return Math.sqrt(dx * dx + dy * dy + dz * dz);
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
