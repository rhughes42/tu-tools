"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.validateToolpath = validateToolpath;
function validateToolpath(points, maxRapidStepMm = 120, minAllowedZ = -100) {
    const issues = [];
    if (points.length < 2) {
        issues.push("Toolpath must contain at least two points.");
        return issues;
    }
    for (let i = 1; i < points.length; i++) {
        const prev = points[i - 1];
        const curr = points[i];
        const step = distanceTo(prev, curr);
        if (step > maxRapidStepMm) {
            issues.push(`Large move detected at segment ${i}: ${step.toFixed(2)} mm.`);
        }
        if (curr.z < minAllowedZ) {
            issues.push(`Z below safe limit at point ${i}: ${curr.z.toFixed(2)}.`);
        }
    }
    return issues;
}
function distanceTo(a, b) {
    const dx = a.x - b.x;
    const dy = a.y - b.y;
    const dz = a.z - b.z;
    return Math.sqrt(dx * dx + dy * dy + dz * dz);
}
