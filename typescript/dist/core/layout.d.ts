import type { LayoutStrategy, Point } from "./types";
export declare function layoutPoints(pts: Point[], strategy: LayoutStrategy, options?: {
    rows?: number;
    cols?: number;
    spacingX?: number;
    spacingY?: number;
}): Point[];
