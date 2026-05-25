import type { CutPhysicsOptions, CutTimeEstimate, Point } from "./types";
export declare function estimateTime(path: Point[], options: CutPhysicsOptions): CutTimeEstimate;
export declare function summarizeTime(est: CutTimeEstimate): string[];
export declare function runPerformanceBaseline(path: Point[], options: CutPhysicsOptions, iterations?: number): number;
