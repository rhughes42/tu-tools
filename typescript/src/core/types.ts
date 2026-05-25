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
  cutFeed: number;
  rapidFeed: number;
  acceleration: number;
  mass: number;
  inertia: number;
  rapidThreshold: number;
  spinupSeconds?: number;
};

export type CutTimeEstimate = {
  cuttingSeconds: number;
  rapidSeconds: number;
  spinupSeconds: number;
  distance: number;
  totalSeconds: number;
};
