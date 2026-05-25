"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.hundeggerProfile = void 0;
exports.hundeggerProfile = {
    name: "hundegger",
    transformCommands(commands) {
        return [
            "(HUNDEGGER PROFILE)",
            ...commands.map((cmd) => cmd.replace("G0 ", "G00 ").replace("G1 ", "G01 ")),
        ];
    },
    validate(commands) {
        const issues = [];
        if (!commands.some((x) => x.toUpperCase().includes("M03"))) {
            issues.push("Hundegger profile expects spindle start command (M03).");
        }
        if (!commands.some((x) => x.toUpperCase().includes("M06") && x.toUpperCase().includes("T"))) {
            issues.push("Hundegger profile expects tool change command (Tn M06).");
        }
        return issues;
    },
};
