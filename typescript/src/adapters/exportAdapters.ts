import type { Point } from "../core/types";

export interface ExportAdapter {
  name: string;
  export(points: Point[], commands: string[]): string;
}

export const plainExportAdapter: ExportAdapter = {
  name: "plain",
  export(_points, commands) {
    return commands.join("\n");
  },
};

export const jsonSimulationExportAdapter: ExportAdapter = {
  name: "json",
  export(points, commands) {
    return JSON.stringify({ points, commands, generatedUtc: new Date().toISOString() });
  },
};

export function resolveExportAdapter(name?: string): ExportAdapter {
  return (name ?? "").toLowerCase() === "json" ? jsonSimulationExportAdapter : plainExportAdapter;
}
