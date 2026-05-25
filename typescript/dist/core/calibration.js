"use strict";
Object.defineProperty(exports, "__esModule", { value: true });
exports.applyCalibration = applyCalibration;
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
