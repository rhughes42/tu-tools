import type { Point } from "../core/types";
export interface ExportAdapter {
    name: string;
    export(points: Point[], commands: string[]): string;
}
export declare const plainExportAdapter: ExportAdapter;
export declare const jsonSimulationExportAdapter: ExportAdapter;
export declare function resolveExportAdapter(name?: string): ExportAdapter;
