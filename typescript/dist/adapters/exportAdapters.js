"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.jsonSimulationExportAdapter = exports.plainExportAdapter = void 0;
exports.resolveExportAdapter = resolveExportAdapter;
exports.plainExportAdapter = {
    name: "plain",
    export(_points, commands) {
        return commands.join("\n");
    },
};
exports.jsonSimulationExportAdapter = {
    name: "json",
    export(points, commands) {
        return JSON.stringify({ points, commands, generatedUtc: new Date().toISOString() });
    },
};
function resolveExportAdapter(name) {
    return (name !== null && name !== void 0 ? name : "").toLowerCase() === "json" ? exports.jsonSimulationExportAdapter : exports.plainExportAdapter;
}
