import type { Point } from "./types";
export declare function applyCalibration(pts: Point[], offset: {
    x?: number;
    y?: number;
    z?: number;
    rotationDeg?: number;
    scale?: number;
}): Point[];
