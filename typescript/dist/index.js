"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.applyCalibration = applyCalibration;
exports.layoutPoints = layoutPoints;
exports.renderCustomCommand = renderCustomCommand;
exports.toolHeader = toolHeader;
exports.estimateTime = estimateTime;
exports.summarizeTime = summarizeTime;
function applyCalibration(pts, offset) {
    var _a, _b, _c, _d, _e;
    const ox = (_a = offset.x) !== null && _a !== void 0 ? _a : 0;
    const oy = (_b = offset.y) !== null && _b !== void 0 ? _b : 0;
    const oz = (_c = offset.z) !== null && _c !== void 0 ? _c : 0;
    const rotation = (((_d = offset.rotationDeg) !== null && _d !== void 0 ? _d : 0) * Math.PI) / 180;
    const scale = (_e = offset.scale) !== null && _e !== void 0 ? _e : 1;
    const cos = Math.cos(rotation) * scale;
    const sin = Math.sin(rotation) * scale;
    return pts.map((p) => ({
        x: p.x * cos - p.y * sin + ox,
        y: p.x * sin + p.y * cos + oy,
        z: p.z * scale + oz,
    }));
}
function layoutPoints(pts, strategy, options = {}) {
    var _a, _b, _c, _d;
    const rows = Math.max(1, (_a = options.rows) !== null && _a !== void 0 ? _a : 1);
    const cols = Math.max(1, (_b = options.cols) !== null && _b !== void 0 ? _b : 1);
    const sx = (_c = options.spacingX) !== null && _c !== void 0 ? _c : 0;
    const sy = (_d = options.spacingY) !== null && _d !== void 0 ? _d : 0;
    switch (strategy) {
        case "grid":
            return grid(pts, rows, cols, sx, sy);
        case "mirrorX":
            return mirror(pts, true);
        case "mirrorY":
            return mirror(pts, false);
        case "rotate90":
            return rotate(pts, 90);
        case "rotate180":
            return rotate(pts, 180);
        case "rotate270":
            return rotate(pts, 270);
        case "none":
        default:
            return pts;
    }
}
function renderCustomCommand(template, parameters) {
    return Object.keys(parameters).reduce((cmd, key) => {
        return cmd.replace(new RegExp(`\\{${key}\\}`, "g"), String(parameters[key]));
    }, template);
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
function estimateTime(path, options) {
    var _a, _b;
    if (path.length < 2) {
        return { cuttingSeconds: 0, rapidSeconds: 0, spinupSeconds: (_a = options.spinupSeconds) !== null && _a !== void 0 ? _a : 3, distance: 0, totalSeconds: 0 };
    }
    const accel = options.acceleration / Math.max(0.5, 1 + options.mass * options.inertia);
    let cutting = 0;
    let rapids = 0;
    let distance = 0;
    for (let i = 1; i < path.length; i++) {
        const a = path[i - 1];
        const b = path[i];
        const d = distanceTo(a, b);
        distance += d;
        const isRapid = d >= options.rapidThreshold || a.z < b.z;
        const feed = (isRapid ? options.rapidFeed : options.cutFeed) / 60;
        const accelTime = feed / accel;
        const accelDist = 0.5 * accel * accelTime * accelTime;
        let segment;
        if (2 * accelDist >= d) {
            segment = 2 * Math.sqrt(d / accel);
        }
        else {
            const cruise = d - 2 * accelDist;
            segment = 2 * accelTime + cruise / feed;
        }
        if (isRapid)
            rapids += segment;
        else
            cutting += segment;
    }
    const spin = Math.max(0, (_b = options.spinupSeconds) !== null && _b !== void 0 ? _b : 3);
    return {
        cuttingSeconds: cutting,
        rapidSeconds: rapids,
        spinupSeconds: spin,
        distance,
        totalSeconds: cutting + rapids + spin,
    };
}
function summarizeTime(est) {
    return [
        `Distance: ${est.distance.toFixed(2)} mm`,
        `Cutting: ${est.cuttingSeconds.toFixed(2)} s`,
        `Rapids: ${est.rapidSeconds.toFixed(2)} s`,
        `Spin-up: ${est.spinupSeconds.toFixed(2)} s`,
        `Total: ${est.totalSeconds.toFixed(2)} s`,
    ];
}
// Helpers
function distanceTo(a, b) {
    const dx = a.x - b.x;
    const dy = a.y - b.y;
    const dz = a.z - b.z;
    return Math.sqrt(dx * dx + dy * dy + dz * dz);
}
function grid(pts, rows, cols, sx, sy) {
    const result = [];
    for (let r = 0; r < rows; r++) {
        for (let c = 0; c < cols; c++) {
            const dx = c * sx;
            const dy = r * sy;
            pts.forEach((p) => result.push({ x: p.x + dx, y: p.y + dy, z: p.z }));
        }
    }
    return result;
}
function mirror(pts, mirrorX) {
    return [
        ...pts,
        ...pts.map((p) => ({
            x: mirrorX ? -p.x : p.x,
            y: mirrorX ? p.y : -p.y,
            z: p.z,
        })),
    ];
}
function rotate(pts, deg) {
    const rad = (deg * Math.PI) / 180;
    const cos = Math.cos(rad);
    const sin = Math.sin(rad);
    return pts.map((p) => ({
        x: p.x * cos - p.y * sin,
        y: p.x * sin + p.y * cos,
        z: p.z,
    }));
}
