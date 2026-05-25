export type { Point, ToolConfiguration, LayoutStrategy, CutPhysicsOptions, CutTimeEstimate } from "./core/types";

export { applyCalibration } from "./core/calibration";
export { layoutPoints } from "./core/layout";
export { sequenceNearestNeighbor } from "./core/sequencing";
export { validateToolpath } from "./core/validation";
export { estimateTime, summarizeTime, runPerformanceBaseline } from "./core/time";
export { normalizeCommands, addLineNumbers, renderCustomCommandSafe, toolHeader } from "./core/programTransforms";

export type { MachineProfile } from "./machineProfiles/types";
export { hundeggerProfile } from "./machineProfiles/hundegger";
export { resolveMachineProfile } from "./machineProfiles/registry";

export type { ExportAdapter } from "./adapters/exportAdapters";
export { plainExportAdapter, jsonSimulationExportAdapter, resolveExportAdapter } from "./adapters/exportAdapters";

export type { TelemetryContext, TelemetryEvent, TelemetrySink } from "./observability/telemetry";
export { ConsoleTelemetrySink, SentryCompatibleTelemetrySink, TelemetryClient } from "./observability/telemetry";

// Backward-compatible alias for existing API name.
export { renderCustomCommandSafe as renderCustomCommand } from "./core/programTransforms";
