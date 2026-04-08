export type Point = {
    x: number;
    y: number;
    z: number;
};
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
export type LayoutStrategy = "grid" | "mirrorX" | "mirrorY" | "rotate90" | "rotate180" | "rotate270" | "none";
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
export declare function applyCalibration(pts: Point[], offset: {
    x?: number;
    y?: number;
    z?: number;
    rotationDeg?: number;
    scale?: number;
}): Point[];
export declare function layoutPoints(pts: Point[], strategy: LayoutStrategy, options?: {
    rows?: number;
    cols?: number;
    spacingX?: number;
    spacingY?: number;
}): Point[];
export declare function renderCustomCommand(template: string, parameters: Record<string, string | number>): string;
export declare function toolHeader(cfg: ToolConfiguration): string[];
export declare function estimateTime(path: Point[], options: CutPhysicsOptions): CutTimeEstimate;
export declare function summarizeTime(est: CutTimeEstimate): string[];
