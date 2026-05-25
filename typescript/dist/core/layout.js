"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.layoutPoints = layoutPoints;
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
