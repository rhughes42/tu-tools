"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.normalizeCommands = normalizeCommands;
exports.addLineNumbers = addLineNumbers;
exports.renderCustomCommandSafe = renderCustomCommandSafe;
exports.toolHeader = toolHeader;
function normalizeCommands(commands) {
    return [...new Set(commands.map((x) => x.trim()).filter((x) => x.length > 0))];
}
function addLineNumbers(commands, start = 10, step = 10) {
    let line = start;
    return commands.map((cmd) => {
        const result = `N${line} ${cmd}`;
        line += step;
        return result;
    });
}
function renderCustomCommandSafe(template, parameters) {
    if (!template.trim())
        return "";
    let result = Object.keys(parameters).reduce((cmd, key) => {
        return cmd.replace(new RegExp(`\\{${key}\\}`, "g"), String(parameters[key]));
    }, template);
    // Remove unresolved placeholders.
    result = result.replace(/\{[^{}]+\}/g, "");
    return result.trim();
}
function toolHeader(cfg) {
    const lines = [
        `( Tool ${cfg.toolNumber} | Dia ${cfg.diameter.toFixed(2)}mm | ${cfg.material} )`,
        `T${cfg.toolNumber} M06`,
        `S${cfg.spindleRpm.toFixed(0)} M03`,
        `F${cfg.feedRate.toFixed(1)}`,
        `( Plunge ${cfg.plungeRate.toFixed(1)} mm/min )`,
    ];
    if (cfg.coolant)
        lines.push("M08");
    return lines;
}
